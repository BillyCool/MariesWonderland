using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(QuestId), nameof(QuestMissionId))]
public class EntityIUserQuestMission : IUserEntity
{
    public long UserId { get; set; }

    public int QuestId { get; set; }

    public int QuestMissionId { get; set; }

    public int ProgressValue { get; set; }

    public bool IsClear { get; set; }

    public long LatestClearDatetime { get; set; }

    public long LatestVersion { get; set; }
}
