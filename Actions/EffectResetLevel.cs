using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectResetLevel : Effect
{
    public override string Code { get; } = "reset";

    public override void Start()
    {
        base.Start();
        if (!Active || (Level == null) || (!Player.Active)) { return; }

        SaveData.Instance.LastArea = AreaKey.None;
        SaveData.Instance.LastArea_Safe = AreaKey.None;
        if (Level.StartPosition.HasValue) { Player.Position = Level.StartPosition.Value; }
        Player.Die(Vector2.Zero, true, true);
    }
}