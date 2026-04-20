using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMLimitedOpenTextGroupTable : TableBase<EntityMLimitedOpenTextGroup>
{
    private readonly Func<EntityMLimitedOpenTextGroup, (int, int)> primaryIndexSelector;

    public EntityMLimitedOpenTextGroupTable(EntityMLimitedOpenTextGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.LimitedOpenTextGroupId, element.SortOrder);
    }
}
