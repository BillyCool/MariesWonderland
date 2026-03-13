using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMLibraryStoryGroupTable : TableBase<EntityMLibraryStoryGroup>
{
    private readonly Func<EntityMLibraryStoryGroup, (int, int)> primaryIndexSelector;

    public EntityMLibraryStoryGroupTable(EntityMLibraryStoryGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestId, element.SortOrder);
    }

    public bool TryFindByQuestIdAndSortOrder(ValueTuple<int, int> key, out EntityMLibraryStoryGroup result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);
}
