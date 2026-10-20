using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEnemySizeTypeConfig))]
[PrimaryKey(nameof(CostumeAssetCategoryType), nameof(ActorSkeletonId))]
public class EntityMBattleEnemySizeTypeConfig
{
    [Key(0)] public CostumeAssetCategoryType CostumeAssetCategoryType { get; set; }

    [Key(1)] public int ActorSkeletonId { get; set; }

    [Key(2)] public EnemySizeType EnemySizeType { get; set; }
}
