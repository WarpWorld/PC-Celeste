using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectSpriteWar : Effect
{
    public override string Code { get; } = "sprite";

    public override string Group { get; } = "sprite";

    public override EffectType Type { get; } = EffectType.BidWar;

    public override Type[] ParameterTypes { get; } = { typeof(string) };

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || (Player == null)) { return; }

        //Log.Debug($"Sprite parameter: {Parameters[0]}");
        PlayerSpriteMode spriteMode =
            (string.Equals(Parameters[0].ToString(), "madeline", StringComparison.InvariantCultureIgnoreCase))
                ? PlayerSpriteMode.Madeline
                : PlayerSpriteMode.Badeline;

        if (Player.DefaultSpriteMode != spriteMode)
        {
            Player.DefaultSpriteMode = spriteMode;
            Player.ResetSprite(spriteMode);
        }
    }
}

//old-style CC1 definitions
/*
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

// ReSharper disable once UnusedMember.Global
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
*/