using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterViewerActorIcon))]
public class EntityMCharacterViewerActorIcon
{
    [Key(0)] public CostumeAssetCategoryType CostumeAssetCategoryType { get; set; }

    [Key(1)] public int SkeletonId { get; set; }

    [Key(2)] public int AssetVariationId { get; set; }

    [Key(3)] public int OverrideCostumeAssetCategoryType { get; set; }

    [Key(4)] public int OverrideIconSkeletonId { get; set; }

    [Key(5)] public int OverrideIconAssetVariationId { get; set; }
}
