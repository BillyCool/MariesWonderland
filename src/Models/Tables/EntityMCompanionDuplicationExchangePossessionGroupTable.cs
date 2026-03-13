using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionDuplicationExchangePossessionGroupTable : TableBase<EntityMCompanionDuplicationExchangePossessionGroup>
{
    private readonly Func<EntityMCompanionDuplicationExchangePossessionGroup, (int, PossessionType)> primaryIndexSelector;

    public EntityMCompanionDuplicationExchangePossessionGroupTable(EntityMCompanionDuplicationExchangePossessionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CompanionId, element.PossessionType);
    }
}
