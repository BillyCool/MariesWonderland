using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMLibraryMainQuestStoryTable : TableBase<EntityMLibraryMainQuestStory>
{
    private readonly Func<EntityMLibraryMainQuestStory, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMLibraryMainQuestStory, int> secondaryIndexSelector;

    public EntityMLibraryMainQuestStoryTable(EntityMLibraryMainQuestStory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.LibraryMainQuestGroupId, element.SortOrder);
        secondaryIndexSelector = element => element.LibraryMainQuestGroupId;
    }

    public RangeView<EntityMLibraryMainQuestStory> FindByLibraryMainQuestGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
