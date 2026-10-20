using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserMainQuestProgressStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public int HeadQuestSceneId { get; set; }

    public QuestFlowType CurrentQuestFlowType { get; set; }

    public long LatestVersion { get; set; }
}
