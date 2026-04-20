using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMLimitedOpenTextTable : TableBase<EntityMLimitedOpenText>
{
    private readonly Func<EntityMLimitedOpenText, (LimitedOpenTargetType, int)> primaryIndexSelector;

    public EntityMLimitedOpenTextTable(EntityMLimitedOpenText[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.LimitedOpenTargetType, element.TargetId);
    }

    public EntityMLimitedOpenText FindByLimitedOpenTargetTypeAndTargetId(ValueTuple<LimitedOpenTargetType, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(LimitedOpenTargetType, int)>.Default, key);
}
