using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(QuestSceneChoiceGroupingId))]
public class EntityIUserQuestSceneChoice : IUserEntity
{
    public long UserId { get; set; }

    public int QuestSceneChoiceGroupingId { get; set; }

    public int QuestSceneChoiceEffectId { get; set; }

    public long LatestVersion { get; set; }
}
