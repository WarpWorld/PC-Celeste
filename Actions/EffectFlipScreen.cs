﻿using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectFlipScreen : Effect
{
    public override string Code { get; } = "flipscreen";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(15);

    public override string[] Mutex { get; } = { "screen" };

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || (Player == null)) { return; }

        SaveData.Instance.Assists.MirrorMode = true;
    }

    public override void End()
    {
        base.End();
        SaveData.Instance.Assists.MirrorMode = false;
    }
}