using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectNoGravity : Effect
{
    public override string Code { get; } = "nogravity";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    private static readonly FieldInfo _gravity = typeof(Player).GetField("Gravity", BindingFlags.Static | BindingFlags.NonPublic);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (!(Engine.Scene is Level level)) || (Player == null)) { return; }

        _gravity.SetValue(Player, 0f);
    }

    public override void End()
    {
        base.End();
        if (Player != null) { _gravity.SetValue(Player, 900f); }
    }
}