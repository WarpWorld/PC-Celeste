using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CrowdControl.Actions
{
    // ReSharper disable once UnusedMember.Global
    public class EffectNoStamina : Effect
    {
        public override string Code { get; } = "nostamina";

        public override EffectType Type { get; } = EffectType.Timed;

        public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(15);

        public override string[] Mutex { get; } = { "stamina" };

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Player.Stamina = 0f;
        }
    }
}
