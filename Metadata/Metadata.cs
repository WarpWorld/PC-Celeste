using ConnectorLib.JSON;

namespace Celeste.Mod.CrowdControl.Metadata;

public abstract class Metadata
{
    public abstract string Key { get; }
    public abstract EffectResponseMetadata TryGetValue();

    protected Player? Player => CrowdControlHelper.Instance.Player;
}