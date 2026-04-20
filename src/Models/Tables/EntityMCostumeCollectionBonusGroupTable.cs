using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeCollectionBonusGroupTable : TableBase<EntityMCostumeCollectionBonusGroup>
{
    private readonly Func<EntityMCostumeCollectionBonusGroup, (int, int)> primaryIndexSelector;

    public EntityMCostumeCollectionBonusGroupTable(EntityMCostumeCollectionBonusGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CollectionBonusGroupId, element.CostumeId);
    }
}
