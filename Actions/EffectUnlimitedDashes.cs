using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectUnlimitedDashes : Effect
{
    public override string Code { get; } = "dashes";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Player.RefillDash();
    }
}