using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAssetEffectTable : TableBase<EntityMAssetEffect>
{
    private readonly Func<EntityMAssetEffect, int> primaryIndexSelector;

    public EntityMAssetEffectTable(EntityMAssetEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AssetEffectId;
    }
}
