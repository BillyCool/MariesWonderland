using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAssetBackgroundTable : TableBase<EntityMAssetBackground>
{
    private readonly Func<EntityMAssetBackground, int> primaryIndexSelector;

    public EntityMAssetBackgroundTable(EntityMAssetBackground[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AssetBackgroundId;
    }

    public EntityMAssetBackground FindByAssetBackgroundId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
