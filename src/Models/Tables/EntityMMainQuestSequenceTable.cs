using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMainQuestSequenceTable : TableBase<EntityMMainQuestSequence>
{
    private readonly Func<EntityMMainQuestSequence, (int, int)> primaryIndexSelector;

    public EntityMMainQuestSequenceTable(EntityMMainQuestSequence[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.MainQuestSequenceId, element.SortOrder);
    }

    public EntityMMainQuestSequence FindClosestByMainQuestSequenceIdAndSortOrder(ValueTuple<int, int> key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, selectLower);

    public RangeView<EntityMMainQuestSequence> FindRangeByMainQuestSequenceIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
