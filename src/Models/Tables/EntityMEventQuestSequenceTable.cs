using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestSequenceTable : TableBase<EntityMEventQuestSequence>
{
    private readonly Func<EntityMEventQuestSequence, (int, int)> primaryIndexSelector;

    public EntityMEventQuestSequenceTable(EntityMEventQuestSequence[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestSequenceId, element.SortOrder);
    }

    public EntityMEventQuestSequence FindByEventQuestSequenceIdAndSortOrder(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public RangeView<EntityMEventQuestSequence> FindRangeByEventQuestSequenceIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
