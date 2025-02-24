using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectInvertDPad : Effect
{
    public override string Code { get; } = "invertdpad";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(15);

    public override string[] Mutex { get; } = { "dpad" };

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || (Player == null)) { return; }

        Input.MoveX.Inverted = true;
        Input.MoveY.Inverted = true;
        Input.Aim.InvertedX = true;
        Input.Aim.InvertedY = true;
    }

    public override void End()
    {
        base.End();
        Input.MoveX.Inverted = false;
        Input.MoveY.Inverted = false;
        Input.Aim.InvertedX = false;
        Input.Aim.InvertedY = false;

    }
}