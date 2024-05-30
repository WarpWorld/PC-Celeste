﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectorLib.JSON;
using Newtonsoft.Json;

namespace CrowdControl;

public class SimpleTCPClient : IDisposable
{
    private TcpClient _client;
    private readonly SemaphoreSlim _client_lock = new(1);
    private readonly ManualResetEventSlim _ready = new(false);
    private readonly ManualResetEventSlim _error = new(false);

    private static readonly JsonSerializerSettings JSON_SETTINGS = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    public bool Connected { get; private set; }

    private readonly CancellationTokenSource _quitting = new();

    public SimpleTCPClient()
    {
        Task.Factory.StartNew(ConnectLoop, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(Listen, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(KeepAlive, TaskCreationOptions.LongRunning);
    }

    ~SimpleTCPClient() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _quitting.Cancel();
        if (disposing) { _client.Close(); }
    }

    private async void ConnectLoop()
    {
        while (!_quitting.IsCancellationRequested)
        {
            try
            {
                _client = new() { ExclusiveAddressUse = false, LingerState = new(true, 0) };
                await _client.ConnectAsync("127.0.0.1", 58430);
                if (!_client.Connected) { continue; }
                Connected = true;
                try { OnConnected?.Invoke(); }
                catch (Exception e) { Log.Error(e); }
                _ready.Set();
                await _error.WaitHandle.WaitOneAsync(_quitting.Token);
            }
            catch (Exception e) { Log.Error(e); }
            finally
            {
                Connected = false;
                _error.Reset();
                _ready.Reset();
                try { _client.Close(); }
                catch { /**/ }
                if (!_quitting.IsCancellationRequested) { await Task.Delay(TimeSpan.FromSeconds(1)); }
            }
        }
        Connected = false;
    }

    private async void Listen()
    {
        List<byte> mBytes = new();
        byte[] buf = new byte[4096];
        while (!_quitting.IsCancellationRequested)
        {
            try
            {
                if (!(await _ready.WaitHandle.WaitOneAsync(_quitting.Token))) continue;
                Socket socket = _client.Client;

                int bytesRead = socket.Receive(buf);
                //Log.Debug($"Got {bytesRead} bytes from socket.");

                //this is "slow" but the messages are tiny so we don't really care
                foreach (byte b in buf.Take(bytesRead))
                {
                    if (b != 0) { mBytes.Add(b); }
                    else
                    {
                        //Log.Debug($"Got a complete message: {mBytes.ToArray().ToHexadecimalString()}");
                        string json = Encoding.UTF8.GetString(mBytes.ToArray());
                        //Log.Debug($"Got a complete message: {json}");
                        SimpleJSONRequest req = SimpleJSONRequest.Parse(json);
                        //Log.Debug($"Got a request with ID {req.id}.");
                        try { OnRequestReceived?.Invoke(req); }
                        catch (Exception e) { Log.Error(e); }
                        mBytes.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                _error.Set();
            }
            finally { if (!_quitting.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(1)); }
        }
    }

    private async void KeepAlive()
    {
        while (!_quitting.IsCancellationRequested)
        {
            try
            {
                if (Connected) { await Respond(new() { id = 0, type = ResponseType.KeepAlive }); }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (Exception e)
            {
                Log.Error(e);
                _error.Set();
            }
            finally { if (!_quitting.IsCancellationRequested) { await Task.Delay(TimeSpan.FromSeconds(1)); } }
        }
    }

    public event Action<SimpleJSONRequest> OnRequestReceived;
    public event Action OnConnected;

    public async Task<bool> Respond(EffectResponse response)
    {
        string json = JsonConvert.SerializeObject(response, JSON_SETTINGS);
        byte[] buffer = Encoding.UTF8.GetBytes(json + '\0');
        Socket socket = _client.Client;
        await _client_lock.WaitAsync();
        try
        {
            int bytesSent = socket.Send(buffer);
            return bytesSent > 0;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return false;
        }
        finally { _client_lock.Release(); }
    }
}