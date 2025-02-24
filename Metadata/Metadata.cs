using ConnectorLib.JSON;

namespace Celeste.Mod.CrowdControl.Metadata;

public abstract class Metadata
{
    public abstract string Key { get; }
    public abstract DataResponse TryGetValue();

    protected Level? Level => CrowdControlHelper.Instance.Level;
    protected Player? Player => CrowdControlHelper.Instance.Player;
}