using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSideStoryQuestSceneTable : TableBase<EntityMSideStoryQuestScene>
{
    private readonly Func<EntityMSideStoryQuestScene, (int, int)> primaryIndexSelector;

    public EntityMSideStoryQuestSceneTable(EntityMSideStoryQuestScene[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SideStoryQuestId, element.SideStoryQuestSceneId);
    }

    public EntityMSideStoryQuestScene FindBySideStoryQuestIdAndSideStoryQuestSceneId(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
