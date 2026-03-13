using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestSceneOutgameBlendshapeMotionTable : TableBase<EntityMQuestSceneOutgameBlendshapeMotion>
{
    private readonly Func<EntityMQuestSceneOutgameBlendshapeMotion, int> primaryIndexSelector;

    public EntityMQuestSceneOutgameBlendshapeMotionTable(EntityMQuestSceneOutgameBlendshapeMotion[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestSceneId;
    }

    public EntityMQuestSceneOutgameBlendshapeMotion FindByQuestSceneId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
