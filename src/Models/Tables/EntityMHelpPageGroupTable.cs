using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMHelpPageGroupTable : TableBase<EntityMHelpPageGroup>
{
    private readonly Func<EntityMHelpPageGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMHelpPageGroup, int> secondaryIndexSelector;

    public EntityMHelpPageGroupTable(EntityMHelpPageGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.HelpPageGroupId, element.SortOrder);
        secondaryIndexSelector = element => element.HelpPageGroupId;
    }

    public RangeView<EntityMHelpPageGroup> FindRangeByHelpPageGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
