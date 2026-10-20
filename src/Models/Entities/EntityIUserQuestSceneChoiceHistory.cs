using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(QuestSceneChoiceEffectId))]
public class EntityIUserQuestSceneChoiceHistory : IUserEntity
{
    public long UserId { get; set; }

    public int QuestSceneChoiceEffectId { get; set; }

    public long ChoiceDatetime { get; set; }

    public long LatestVersion { get; set; }
}
