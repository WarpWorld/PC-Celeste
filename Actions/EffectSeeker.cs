using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions
{
    // ReSharper disable once UnusedMember.Global
    public class EffectSeeker : Effect
    {
        public override string Code { get; } = "seeker";

        public override EffectType Type { get; } = EffectType.Timed;

        public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

        public Seeker Seeker;

        public virtual Seeker NewSeeker(Vector2 position) => new Seeker(position, new Vector2[0]);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!Active || (!(Engine.Scene is Level level)) || level.Entities.Contains(Seeker) || level.Entities.GetToAdd().Contains(Seeker)) { return; }

            Vector2 position = new Vector2(level.Bounds.Left + (level.Bounds.Width / 2f), level.Bounds.Top + +(level.Bounds.Height / 2f));
            Seeker = NewSeeker(position);
            level.Add(Seeker);
        }

        public override void End()
        {
            base.End();
            if ((Seeker == null) || (!(Engine.Scene is Level level))) { return; }

            level.Remove(Seeker);
            Seeker = null;
        }
    }
}
