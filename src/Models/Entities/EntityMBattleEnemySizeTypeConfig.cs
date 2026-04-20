using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEnemySizeTypeConfig))]
public class EntityMBattleEnemySizeTypeConfig
{
    [Key(0)] public CostumeAssetCategoryType CostumeAssetCategoryType { get; set; }

    [Key(1)] public int ActorSkeletonId { get; set; }

    [Key(2)] public EnemySizeType EnemySizeType { get; set; }
}
