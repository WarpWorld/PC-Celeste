namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectSlow : EffectSpeed
{
    public override string Code { get; } = "slow";

    public override float Rate { get; } = 0.5f;
}