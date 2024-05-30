using ConnectorLib.JSON;
using Monocle;

namespace Celeste.Mod.CrowdControl.Metadata;

public class MetadataDeaths : Metadata
{
    public override string Key => "deaths";

    public override EffectResponseMetadata TryGetValue()
    {
        if (Engine.Scene is not Level level)
            return EffectResponseMetadata.Failure(Key, "Not in a level.");
        //if (Player == null)
        //    return EffectResponseMetadata.Failure(Key, "Player object not found.");

        return EffectResponseMetadata.Success(Key, new
        {
            level = level.Session.DeathsInCurrentLevel,
            total = level.Session.Deaths
        });
    }
}