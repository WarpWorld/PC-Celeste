using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions
{
    // ReSharper disable once UnusedMember.Global
    public class EffectSpriteWarMadeline : Effect
    {
        public override string Code { get; } = "sprite_madeline";

        public override string Group { get; } = "sprite";

        public override EffectType Type { get; } = EffectType.BidWar;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!Active || (!(Engine.Scene is Level level)) || (Player == null)) { return; }

            if (Player.DefaultSpriteMode != PlayerSpriteMode.Madeline)
            {
                Player.DefaultSpriteMode = PlayerSpriteMode.Madeline;
                Player.ResetSprite(PlayerSpriteMode.Madeline);
            }
        }
    }

    public class EffectSpriteWarBadeline : Effect
    {
        public override string Code { get; } = "sprite_badeline";

        public override string Group { get; } = "sprite";

        public override EffectType Type { get; } = EffectType.BidWar;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!Active || (!(Engine.Scene is Level level)) || (Player == null)) { return; }

            if (Player.DefaultSpriteMode != PlayerSpriteMode.MadelineAsBadeline)
            {
                Player.DefaultSpriteMode = PlayerSpriteMode.MadelineAsBadeline;
                Player.ResetSprite(PlayerSpriteMode.MadelineAsBadeline);
            }
        }
    }
}