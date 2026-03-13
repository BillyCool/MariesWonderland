using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeAwakenStatusUpGroupTable : TableBase<EntityMCostumeAwakenStatusUpGroup>
{
    private readonly Func<EntityMCostumeAwakenStatusUpGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMCostumeAwakenStatusUpGroup, int> secondaryIndexSelector;

    public EntityMCostumeAwakenStatusUpGroupTable(EntityMCostumeAwakenStatusUpGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeAwakenStatusUpGroupId, element.SortOrder);
        secondaryIndexSelector = element => element.CostumeAwakenStatusUpGroupId;
    }

    public RangeView<EntityMCostumeAwakenStatusUpGroup> FindRangeByCostumeAwakenStatusUpGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
