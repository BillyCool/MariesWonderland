using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPossessionAutoConvertTable : TableBase<EntityIUserPossessionAutoConvert>
{
    private readonly Func<EntityIUserPossessionAutoConvert, (long, PossessionType, int)> primaryIndexSelector;

    public EntityIUserPossessionAutoConvertTable(EntityIUserPossessionAutoConvert[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.PossessionType, element.PossessionId);
    }
}
