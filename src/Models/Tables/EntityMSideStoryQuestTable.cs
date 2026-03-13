using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMSideStoryQuestTable : TableBase<EntityMSideStoryQuest>
{
    private readonly Func<EntityMSideStoryQuest, int> primaryIndexSelector;
    private readonly Func<EntityMSideStoryQuest, (int, int)> secondaryIndexSelector;

    public EntityMSideStoryQuestTable(EntityMSideStoryQuest[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SideStoryQuestId;
        secondaryIndexSelector = element => (element.SideStoryQuestType, element.TargetId);
    }

    public EntityMSideStoryQuest FindBySideStoryQuestId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindBySideStoryQuestId(int key, out EntityMSideStoryQuest result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);

    public RangeView<EntityMSideStoryQuest> FindBySideStoryQuestTypeAndTargetId(ValueTuple<int, int> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(int, int)>.Default, key);
}
