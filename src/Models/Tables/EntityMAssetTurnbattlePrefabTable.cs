using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAssetTurnbattlePrefabTable : TableBase<EntityMAssetTurnbattlePrefab>
{
    private readonly Func<EntityMAssetTurnbattlePrefab, int> primaryIndexSelector;

    public EntityMAssetTurnbattlePrefabTable(EntityMAssetTurnbattlePrefab[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AssetTurnbattlePrefabId;
    }
}
