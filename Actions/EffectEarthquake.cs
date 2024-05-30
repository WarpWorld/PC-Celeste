using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectEarthquake: Effect
{
    public override string Code { get; } = "earthquake";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Engine.Scene is not Level level)) return;
        level.Shake();
    }
}