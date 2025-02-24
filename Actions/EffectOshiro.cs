using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectOshiro : Effect
{
    public override string Code { get; } = "oshiro";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public override string[] Mutex { get; } = { "oshiro" };

    public AngryOshiro? Oshiro;

    public virtual AngryOshiro NewOshiro(Vector2 position) => new(position, false);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || Level.Entities.Contains(Oshiro) || Level.Entities.ToAdd.Contains(Oshiro)) { return; }

        Vector2 position = new(Level.Bounds.Left - 32f, Level.Bounds.Top + Level.Bounds.Height / 2f);
        Oshiro = NewOshiro(position);
        Level.Add(Oshiro);
    }

    public override void End()
    {
        base.End();
        if ((Oshiro == null) || (Level == null)) { return; }

        Level.Remove(Oshiro);
        Oshiro = null;
    }
}