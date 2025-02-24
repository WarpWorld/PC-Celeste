using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectHiccups : Effect
{
    public override string Code { get; } = "hiccups";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan HICCUP_INTERVAL = TimeSpan.FromSeconds(2);
    private TimeSpan _last_hiccup = (-HICCUP_INTERVAL);

    public override void Start()
    {
        base.Start();
        _last_hiccup = (-HICCUP_INTERVAL);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || (Player == null)) { return; }

        if ((Elapsed - _last_hiccup) > HICCUP_INTERVAL)
        {
            _last_hiccup = Elapsed;
            Player.HiccupJump();
        }
    }
}