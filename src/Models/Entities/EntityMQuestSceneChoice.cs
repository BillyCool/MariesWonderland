using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestSceneChoice
{
    public int MainFlowQuestSceneId { get; set; }

    public QuestFlowType QuestFlowType { get; set; }

    public int ChoiceNumber { get; set; }

    public int QuestSceneChoiceEffectId { get; set; }
}
