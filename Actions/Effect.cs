using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CrowdControl;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions
{
    public abstract class Effect
    {
        private static int _next_id = 0;
        private uint LocalID { get; } = unchecked((uint) Interlocked.Increment(ref _next_id));

        public abstract string Code { get; }

        protected bool _active = false;
        private readonly object _activity_lock = new object();

        public TimeSpan Elapsed { get; set; }

        protected Player Player => CrowdControlHelper.Instance.Player;

        public virtual EffectType Type { get; } = EffectType.Instant;

        public virtual TimeSpan Duration { get; } = TimeSpan.Zero;

        public virtual Type[] ParameterTypes { get; } = new Type[0];

        protected object[] Parameters { get; private set; } = new object[0];

        public virtual string Group { get; }

        public virtual string[] Mutex { get; } = new string[0];

        private static readonly ConcurrentDictionary<string, bool> _mutexes = new ConcurrentDictionary<string, bool>();

        private static bool TryGetMutexes(IEnumerable<string> mutexes)
        {
            List<string> captured = new List<string>();
            bool result = true;
            foreach (string mutex in mutexes)
            {
                if (_mutexes.TryAdd(mutex, true)) { captured.Add(mutex); }
                else
                {
                    result = false;
                    break;
                }
            }
            if (!result) { FreeMutexes(captured); }
            return result;
        }

        public static void FreeMutexes(IEnumerable<string> mutexes)
        {
            foreach (string mutex in mutexes)
            {
                _mutexes.TryRemove(mutex, out _);
            }
        }

        public enum EffectType : byte
        {
            Instant = 0,
            Timed = 1,
            BidWar = 2
        }

        public bool Active
        {
            get => _active;
            private set
            {
                if (_active == value) { return; }
                _active = value;
                if (value)
                {
                    Elapsed = TimeSpan.Zero;
                    Start();
                }
                else { End(); }
            }
        }

        public virtual void Load() => Log.Debug($"{GetType().Name} was loaded. [{LocalID}]");

        public virtual void Unload() => Log.Debug($"{GetType().Name} was unloaded. [{LocalID}]");

        public virtual void Start() => Log.Debug($"{GetType().Name} was started. [{LocalID}]");

        public virtual void End() => Log.Debug($"{GetType().Name} was stopped. [{LocalID}]");

        public virtual void Update(GameTime gameTime) => Elapsed += gameTime.ElapsedGameTime;

        public virtual void Draw(GameTime gameTime) { }

        public virtual bool IsReady() => (Engine.Scene is Level) && (Player.Active);

        public bool TryStart() => TryStart(new object[0]);

        public bool TryStart(object[] parameters)
        {
            lock (_activity_lock)
            {
                if (Active || (!IsReady())) { return false; }
                if (!TryGetMutexes(Mutex)) { return false; }
                Parameters = parameters;
                Active = true;
                return true;
            }
        }

        public bool TryStop()
        {
            lock (_activity_lock)
            {
                if (!Active) { return false; }
                FreeMutexes(Mutex);
                Active = false;
                return true;
            }
        }
    }
}