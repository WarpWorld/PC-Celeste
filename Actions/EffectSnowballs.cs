using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectSnowballs : Effect
{
    public override string Code { get; } = "snowballs";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public Snowball? Snowball;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || Level.Entities.Contains(Snowball) || Level.Entities.ToAdd.Contains(Snowball)) { return; }

        Snowball = new();
        Level.Add(Snowball);
    }

    public override void End()
    {
        base.End();
        if ((Snowball == null) || (Level == null)) { return; }

        Level.Remove(Snowball);
        Snowball = null;
    }
}