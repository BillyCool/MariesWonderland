using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserQuestSceneChoice
{
    public long UserId { get; set; }

    public int QuestSceneChoiceGroupingId { get; set; }

    public int QuestSceneChoiceEffectId { get; set; }

    public long LatestVersion { get; set; }
}
