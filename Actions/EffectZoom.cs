using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectZoom: Effect
{
    public override string Code { get; } = "zoom";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || (Player == null)) { return; }

        Level.Camera.Zoom = 2f;
        Level.Camera.Approach(Player.Position, 0.1f);
    }

    public override void End()
    {
        base.End();
        if (Level == null) { return; }

        Level.Camera.Zoom = 1f;
    }
}