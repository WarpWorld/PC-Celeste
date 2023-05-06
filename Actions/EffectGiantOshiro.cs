using Microsoft.Xna.Framework;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectGiantOshiro : EffectOshiro
{
    public override string Code { get; } = "oshiro_giant";

    //this inherits the base oshiro's mutex and will queue with it - kat

    private static readonly Vector2 SCALE = Vector2.One * 2;

    public override AngryOshiro NewOshiro(Vector2 position)
    {
        AngryOshiro result = new(position, false) {Sprite = {Scale = SCALE} };
        return result;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Oshiro != null) { Oshiro.Sprite.Scale = SCALE; }
    }
}