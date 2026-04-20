using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthMob))]
public class EntityMEventQuestLabyrinthMob
{
    [Key(0)] public int EventQuestLabyrinthMobId { get; set; }

    [Key(1)] public int MobAssetId { get; set; }

    [Key(2)] public int BeforeStageClearTextAssetId { get; set; }

    [Key(3)] public int AfterStageClearTextAssetId { get; set; }
}
