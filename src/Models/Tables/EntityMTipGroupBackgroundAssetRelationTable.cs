using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMTipGroupBackgroundAssetRelationTable : TableBase<EntityMTipGroupBackgroundAssetRelation>
{
    private readonly Func<EntityMTipGroupBackgroundAssetRelation, (int, int)> primaryIndexSelector;

    public EntityMTipGroupBackgroundAssetRelationTable(EntityMTipGroupBackgroundAssetRelation[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.TipGroupId, element.TipBackgroundAssetId);
    }
}
