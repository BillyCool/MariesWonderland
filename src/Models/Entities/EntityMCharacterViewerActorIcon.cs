using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterViewerActorIcon
{
    public CostumeAssetCategoryType CostumeAssetCategoryType { get; set; }

    public int SkeletonId { get; set; }

    public int AssetVariationId { get; set; }

    public int OverrideCostumeAssetCategoryType { get; set; }

    public int OverrideIconSkeletonId { get; set; }

    public int OverrideIconAssetVariationId { get; set; }
}
