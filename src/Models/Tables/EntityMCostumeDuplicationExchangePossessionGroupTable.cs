using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeDuplicationExchangePossessionGroupTable : TableBase<EntityMCostumeDuplicationExchangePossessionGroup>
{
    private readonly Func<EntityMCostumeDuplicationExchangePossessionGroup, (int, PossessionType, int)> primaryIndexSelector;

    public EntityMCostumeDuplicationExchangePossessionGroupTable(EntityMCostumeDuplicationExchangePossessionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeId, element.PossessionType, element.PossessionId);
    }
}
