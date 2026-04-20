using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMTipGroupBackgroundAssetTable : TableBase<EntityMTipGroupBackgroundAsset>
{
    private readonly Func<EntityMTipGroupBackgroundAsset, (int, string)> primaryIndexSelector;

    public EntityMTipGroupBackgroundAssetTable(EntityMTipGroupBackgroundAsset[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.TipGroupId, element.BackgroundAssetName);
    }
}
