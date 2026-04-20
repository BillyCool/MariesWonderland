using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterViewerActorIconTable : TableBase<EntityMCharacterViewerActorIcon>
{
    private readonly Func<EntityMCharacterViewerActorIcon, (CostumeAssetCategoryType, int, int)> primaryIndexSelector;

    public EntityMCharacterViewerActorIconTable(EntityMCharacterViewerActorIcon[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeAssetCategoryType, element.SkeletonId, element.AssetVariationId);
    }

    public bool TryFindByCostumeAssetCategoryTypeAndSkeletonIdAndAssetVariationId(ValueTuple<CostumeAssetCategoryType, int, int> key, out EntityMCharacterViewerActorIcon result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(CostumeAssetCategoryType, int, int)>.Default, key, out result);
}
