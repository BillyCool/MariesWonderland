using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserLimitedOpenTable : TableBase<EntityIUserLimitedOpen>
{
    private readonly Func<EntityIUserLimitedOpen, (long, LimitedOpenTargetType, int)> primaryIndexSelector;

    public EntityIUserLimitedOpenTable(EntityIUserLimitedOpen[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.LimitedOpenTargetType, element.TargetId);
    }

    public EntityIUserLimitedOpen FindByUserIdAndLimitedOpenTargetTypeAndTargetId(ValueTuple<long, LimitedOpenTargetType, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, LimitedOpenTargetType, int)>.Default, key);
}
