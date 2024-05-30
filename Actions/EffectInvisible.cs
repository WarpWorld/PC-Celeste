using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectInvisible : Effect
{
    public override string Code { get; } = "invisible";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Engine.Scene is not Level level) || (Player == null)) { return; }

        SaveData.Instance.Assists.InvisibleMotion = true;
    }

    public override void End()
    {
        base.End();

        SaveData.Instance.Assists.InvisibleMotion = false;
    }
}