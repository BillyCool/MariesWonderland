using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserQuestSceneChoiceHistory : IUserEntity
{
    public long UserId { get; set; }

    public int QuestSceneChoiceEffectId { get; set; }

    public long ChoiceDatetime { get; set; }

    public long LatestVersion { get; set; }
}
