﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConnectorLib.JSON;
using Microsoft.Xna.Framework;
using Monocle;
using Newtonsoft.Json.Linq;
using Effect = Celeste.Mod.CrowdControl.Actions.Effect;

namespace Celeste.Mod.CrowdControl;

public class CrowdControlHelper : DrawableGameComponent
{
    public static CrowdControlHelper Instance;

    private readonly SimpleTCPClient _client;

    private readonly ConcurrentQueue<GUIMessage> _gui_messages = new();
    private const int MAX_GUI_MESSAGES = 5;

    private static readonly string INITIAL_CONNECT_WARNING = $"This plugin requires the Crowd Control client software.{Environment.NewLine}Please see https://crowdcontrol.live/ for more information.";

    private bool _connected_once = false;

    private GameState? _last_game_state;

    private const float GAME_STATUS_UPDATE_INTERVAL = 1f;
    private float _game_status_update_timer = 0f;

    private class GUIMessage
    {
        public string message;
        public TimeSpan elapsed;
    }

    public bool GameReady = false;

    private bool m_effectsLoaded;

    public Scene? Scene;
    public Level? Level;
    public Player? Player;
    public Camera? Camera;

    public readonly Dictionary<string, Metadata.Metadata> Metadata = new();

    public readonly Dictionary<string, Effect> Effects = new();
    public IEnumerable<Effect> Active => Effects.Select(e => e.Value).Where(e => e.Active);
    public IEnumerable<Effect> ActiveGroup(string group) => Active.Where(e => string.Equals(e.Group, group));

    public static void Add()
    {
        if (Instance != null) return;
        Instance = new(Celeste.Instance);
        Celeste.Instance.Components.Add(Instance);
    }

    ~CrowdControlHelper() => Dispose(false);

    protected override void Dispose(bool disposing)
    {
        try { Log.OnMessage -= OnLogMessage; }
        catch { /**/ }
        base.Dispose(disposing);
    }

    public CrowdControlHelper(Game game) : base(game)
    {
        Instance = this;

#if DEBUG
        Log.OnMessage += OnLogMessage;
#endif
        UpdateOrder = -10000;
        DrawOrder = 10000;

        _client = new();
        _client.OnConnected += ClientConnected;
        _client.OnRequestReceived += ClientRequestReceived;
    }

    private void TryLoadEffects()
    {
        if (m_effectsLoaded) return;
        //this may be GetEntryAssembly or GetExecutingAssembly depending on how the modloader works
        foreach (Type type in Assembly.GetEntryAssembly().GetTypes())
        {
            //Log.Debug($"Loaded type: {type.Name}");
            if (type.IsAbstract) { continue; }

            if (typeof(Effect).IsAssignableFrom(type))
            {
                Effect action = (Effect)type.GetConstructor(Everest._EmptyTypeArray).Invoke(Everest._EmptyObjectArray);
                action.Load();
                //Log.Debug($"Imported effect type: {type.Name}");
                Effects.Add(action.Code, action);
            }

            if (typeof(Metadata.Metadata).IsAssignableFrom(type))
            {
                Metadata.Metadata metadata = (Metadata.Metadata)type.GetConstructor(Type.EmptyTypes).Invoke([]);
                //Log.Debug($"Imported metadata type: {type.Name}");
                Metadata.Add(metadata.Key, metadata);
            }
        }
        m_effectsLoaded = true;
    }

    private void ClientConnected()
    {
        _connected_once = true;
        try { _client.OnConnected -= ClientConnected; }
        catch { /**/ }
    }

    private void OnLogMessage(string s)
    {
        _gui_messages.Enqueue(new() { message = s, elapsed = TimeSpan.Zero });
    }

    public static void Remove()
    {
        if (Instance == null) { return; }

        Instance._client?.Dispose();

        foreach (Effect action in Instance.Effects.Values)
        {
            try
            {
                action.TryStop();
                action.Unload();
            }
            catch (Exception e) { Log.Error(e); }
        }

        Celeste.Instance.Components.Remove(Instance);
        Instance = null;
    }

    private static readonly TimeSpan MAX_GUI_MESSAGE_TIME = TimeSpan.FromSeconds(2);
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        //Log.Message(_game_status_update_timer);
        _game_status_update_timer += Engine.DeltaTime;
        if (_game_status_update_timer >= GAME_STATUS_UPDATE_INTERVAL)
        {
            UpdateGameState();
            _game_status_update_timer = 0f;
        }

        if (!(Engine.Scene is GameLoader)) GameReady = true;
        if (!GameReady) return;

        Scene = Engine.Scene;
        Level = Scene as Level;
        Player = Scene?.Tracker?.GetEntity<Player>();
        Camera = Level?.Camera;

        TryLoadEffects();
        if (m_effectsLoaded) foreach (Effect action in Active)
        {
            if (!action.Active) continue;
            try
            {
                switch (action.Type)
                {
                    case Effect.EffectType.Timed:
                        TimeSpan timeRemaining = action.Duration - action.Elapsed;
                        if (timeRemaining > TimeSpan.Zero)
                        {
                            action.Update(gameTime);
                            if ((Level == null) || Level.InCutscene || action.Paused)
                            {
                                if (action.IsTimerTicking)
                                {
                                    action.IsTimerTicking = false;
                                    Respond(action.CurrentRequest, EffectStatus.Paused, timeRemaining).Forget();
                                }
                                action.Elapsed -= gameTime.ElapsedGameTime;
                            }
                            else if (!action.IsTimerTicking)
                            {
                                action.IsTimerTicking = true;
                                Respond(action.CurrentRequest, EffectStatus.Resumed, timeRemaining).Forget();
                            }
                        }
                        else action.TryStop();
                        break;
                    case Effect.EffectType.BidWar:
                        action.Update(gameTime);
                        break;
                    default:
                        action.Update(gameTime);
                        action.TryStop();
                        break;
                }
            }
            catch (Exception e) { Log.Error(e); }
        }

        while (_gui_messages.Count > MAX_GUI_MESSAGES) { _gui_messages.TryDequeue(out _); }
        foreach (var gm in _gui_messages) { gm.elapsed += gameTime.ElapsedGameTime; }
        while (_gui_messages.TryPeek(out var g) && (g.elapsed > MAX_GUI_MESSAGE_TIME)) { _gui_messages.TryDequeue(out _); }
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        // Note: This runs earlier than the game finishes loading!
        if (!GameReady) { return; }

        Monocle.Draw.SpriteBatch.Begin();

        StringBuilder sb = new();
        if (!_client.Connected) { sb.AppendLine("Crowd Control - Not Connected"); }
        if (!_connected_once) { sb.AppendLine(INITIAL_CONNECT_WARNING); }
        foreach (var msg in _gui_messages.Take(MAX_GUI_MESSAGES)) { sb.AppendLine(msg.message); }

        ActiveFont.DrawOutline(
            sb.ToString(),
            Vector2.Zero,
            Vector2.Zero,
            Vector2.One * 0.5f,
            Color.White,
            1f,
            Color.Black
        );

        foreach (Effect action in Active)
        {
            try { action.Draw(gameTime); }
            catch (Exception e) { Log.Error(e); }
        }
        Monocle.Draw.SpriteBatch.End();
    }

    [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
    private void ClientRequestReceived(SimpleJSONRequest request)
    {
        if (request is EffectRequest effectRequest)
        {
            switch (effectRequest.type)
            {
                case RequestType.Test:
                    HandleEffectTest(effectRequest);
                    return;
                case RequestType.Start:
                    HandleEffectStart(effectRequest);
                    return;
                case RequestType.Stop:
                    HandleEffectStop(effectRequest);
                    return;
                default:
                    return;
            }
        }
        else if (request.type == RequestType.GameUpdate) UpdateGameState(true);
        //...
    }

    private void HandleEffectStart(EffectRequest request)
    {
        Log.Debug($"Got an effect start request [{request.id}:{request.code}].");
        if (!Effects.TryGetValue(request.code, out Effect effect))
        {
            Log.Error($"Effect {request.code} not found. Available effects: {string.Join(", ", Effects.Keys)}");
            //could not find the effect
            Respond(request, EffectStatus.Unavailable).Forget();
            return;
        }

        if (((request.parameters as JArray)?.Count ?? 0) < effect.ParameterTypes.Length)
        {
            Respond(request, EffectStatus.Failure).Forget();
            return;
        }

        if (effect.Type == Effect.EffectType.BidWar)
        {
            foreach (Effect e in ActiveGroup(effect.Group)) e.TryStop();
        }
        if (!effect.TryStart(request))
        {
            //Log.Debug($"Effect {request.code} could not start.");
            Respond(request, EffectStatus.Retry).Forget();
            return;
        }

        Log.Debug($"Effect {request.code} started.");
        Respond(request, EffectStatus.Success, ((effect.Type == Effect.EffectType.Timed) ? effect.Duration : null)).Forget();
    }

    private void HandleEffectStop(EffectRequest request)
    {
        Log.Debug($"Got an effect stop request [{request.id}:{request.code}].");
        if (!Effects.TryGetValue(request.code, out Effect effect))
        {
            Log.Error($"Effect {request.code} not found. Available effects: {string.Join(", ", Effects.Keys)}");
            //could not find the effect
            return;
        }

        if (!effect.TryStop())
        {
            Log.Debug($"Effect {request.code} failed to stop.");
            return;
        }

        Log.Debug($"Effect {request.code} stopped.");
        Respond(request, EffectStatus.Success).Forget();
    }
    private void HandleEffectTest(EffectRequest request)
    {
        Log.Debug($"Got an effect test request [{request.id}:{request.code}].");
        if (!Effects.TryGetValue(request.code, out Effect effect))
        {
            Log.Error($"Effect {request.code} not found. Available effects: {string.Join(", ", Effects.Keys)}");
            //could not find the effect
            Respond(request, EffectStatus.Unavailable).Forget();
            return;
        }

        if (((request.parameters as JArray)?.Count ?? 0) < effect.ParameterTypes.Length)
        {
            Respond(request, EffectStatus.Failure).Forget();
            return;
        }

        if (!effect.IsReady())
        {
            //Log.Debug($"Effect {request.code} was not ready.");
            Respond(request, EffectStatus.Retry).Forget();
            return;
        }

        Log.Debug($"Effect {request.code} is ready.");
        Respond(request, EffectStatus.Success, ((effect.Type == Effect.EffectType.Timed) ? effect.Duration : null)).Forget();
    }

    //in some other situation we might do a more advanced strategy where certain effects might
    //want to include additional fields, but we're going to send these for all requests
    private Dictionary<string, DataResponse> GetMetadata()
        => Metadata.Select(m => new KeyValuePair<string, DataResponse>(m.Key, m.Value.TryGetValue())).ToDictionary();

    private void UpdateGameState(bool force = false)
    {
        if (Level == null) UpdateGameState(GameState.WrongMode, force).Forget();
        else if (Level.Paused) UpdateGameState(GameState.Paused, force).Forget();
        //else if (!Engine.Scene.Focused) UpdateGameState(GameState.NotFocused, force).Forget();
        else UpdateGameState(GameState.Ready, force).Forget();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<bool> UpdateGameState(GameState newState, bool force) => UpdateGameState(newState, null, force);
    private async Task<bool> UpdateGameState(GameState newState, string? message = null, bool force = false)
    {
        if (force || (_last_game_state != newState))
        {
            _last_game_state = newState;
            return await _client.Respond(new GameUpdate(newState, message));
        }
        return true;
    }
    
    private static readonly TimeSpan START_WAIT_INTERVAL = TimeSpan.FromMilliseconds(20);
    private static readonly TimeSpan MAX_START_WAIT = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MAX_EFFECT_LIFETIME = TimeSpan.FromMinutes(30);
    
    private readonly ConcurrentDictionary<uint, DateTimeOffset> _allow_timed_responses = new();
    private bool IsPrematureTimedResponse(EffectRequest request, EffectStatus result)
    {
        if (result == EffectStatus.Success)
        {
            _allow_timed_responses[request.id] = DateTimeOffset.UtcNow;
            return false;
        }
        if (result is not (EffectStatus.Paused or EffectStatus.Resumed or EffectStatus.Finished)) return false;
        if (!_allow_timed_responses.TryGetValue(request.id, out DateTimeOffset setOn)) return true;
        if ((DateTimeOffset.UtcNow - setOn) > MAX_EFFECT_LIFETIME)
        {
            _allow_timed_responses.TryRemove(request.id, out _);
            return true;
        }
        _allow_timed_responses[request.id] = DateTimeOffset.UtcNow;
        return false;
    }

    private async Task<bool> Respond(EffectRequest request, EffectStatus result, TimeSpan? timeRemaining = null, string message = "")
    {
        try
        {
            TimeSpan timeWaited = TimeSpan.Zero;
            while (timeWaited < MAX_START_WAIT)
            {
                if (_client.Connected && !IsPrematureTimedResponse(request, result)) break;
                await Task.Delay(START_WAIT_INTERVAL);
                timeWaited += START_WAIT_INTERVAL;
            }
            
            return await _client.Respond(new EffectResponse
            {
                id = request.id,
                status = result,
                timeRemaining = ((long?)timeRemaining?.TotalMilliseconds) ?? 0L,
                message = message,
                type = ResponseType.EffectRequest,
                metadata = GetMetadata()
            });
        }
        catch (Exception e)
        {
            Log.Error(e);
            return false;
        }
    }
}