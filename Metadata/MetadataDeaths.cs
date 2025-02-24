using ConnectorLib.JSON;
using Monocle;

namespace Celeste.Mod.CrowdControl.Metadata;

public class MetadataDeaths : Metadata
{
    public override string Key => "deaths";

    public override DataResponse TryGetValue()
    {
        if (Level == null)
            return DataResponse.Failure(Key, "Not in a level.");
        //if (Player == null)
        //    return EffectResponseMetadata.Failure(Key, "Player object not found.");

        return DataResponse.Success(Key, new
        {
            level = Level.Session.DeathsInCurrentLevel,
            total = Level.Session.Deaths
        });
    }
}