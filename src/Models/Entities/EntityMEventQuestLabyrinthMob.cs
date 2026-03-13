using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLabyrinthMob
{
    public int EventQuestLabyrinthMobId { get; set; }

    public int MobAssetId { get; set; }

    public int BeforeStageClearTextAssetId { get; set; }

    public int AfterStageClearTextAssetId { get; set; }
}
