using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestSceneChoiceTable : TableBase<EntityMQuestSceneChoice>
{
    private readonly Func<EntityMQuestSceneChoice, (int, QuestFlowType, int)> primaryIndexSelector;

    public EntityMQuestSceneChoiceTable(EntityMQuestSceneChoice[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.MainFlowQuestSceneId, element.QuestFlowType, element.ChoiceNumber);
    }
}
