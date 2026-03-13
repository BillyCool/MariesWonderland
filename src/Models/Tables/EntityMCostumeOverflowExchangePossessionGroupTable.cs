using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeOverflowExchangePossessionGroupTable : TableBase<EntityMCostumeOverflowExchangePossessionGroup>
{
    private readonly Func<EntityMCostumeOverflowExchangePossessionGroup, (int, int)> primaryIndexSelector;

    public EntityMCostumeOverflowExchangePossessionGroupTable(EntityMCostumeOverflowExchangePossessionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.MaterialId, element.SortOrder);
    }
}
