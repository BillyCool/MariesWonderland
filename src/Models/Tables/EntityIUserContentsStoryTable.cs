using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserContentsStoryTable : TableBase<EntityIUserContentsStory>
{
    private readonly Func<EntityIUserContentsStory, (long, int)> primaryIndexSelector;

    public EntityIUserContentsStoryTable(EntityIUserContentsStory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.ContentsStoryId);
    }

    public EntityIUserContentsStory FindByUserIdAndContentsStoryId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndContentsStoryId(ValueTuple<long, int> key, out EntityIUserContentsStory result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
