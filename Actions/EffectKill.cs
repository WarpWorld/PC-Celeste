using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectKill : Effect
{
    public override string Code { get; } = "kill";

    public override string[] Mutex { get; } = { "life" };

    public override void Start()
    {
        base.Start();
        if (!Active || (Engine.Scene is not Level level) || (!Player.Active)) { return; }

        Player.Die(Vector2.Zero, true, true);
    }
}