using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectSeeker : Effect
{
    public override string Code { get; } = "seeker";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public Seeker Seeker;

    public virtual Seeker NewSeeker(Vector2 position) => new(position, new Vector2[0]);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || Level.Entities.Contains(Seeker) || Level.Entities.ToAdd.Contains(Seeker)) { return; }

        Vector2 position = new(Level.Bounds.Left + (Level.Bounds.Width / 2f), Level.Bounds.Top + +(Level.Bounds.Height / 2f));
        Seeker = NewSeeker(position);
        Level.Add(Seeker);
    }

    public override void End()
    {
        base.End();
        if ((Seeker == null) || (Level == null)) { return; }

        Level.Remove(Seeker);
        Seeker = null;
    }
}