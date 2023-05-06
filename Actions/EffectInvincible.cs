using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectInvincible : Effect
{
    public override string Code { get; } = "invincible";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public override string[] Mutex { get; } = { "life" };

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (!(Engine.Scene is Level level)) || (Player == null)) { return; }

        SaveData.Instance.Assists.Invincible = true;
    }

    public override void End()
    {
        base.End();

        SaveData.Instance.Assists.Invincible = false;
    }
}