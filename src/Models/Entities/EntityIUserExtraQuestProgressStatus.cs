using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserExtraQuestProgressStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentQuestId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public int HeadQuestSceneId { get; set; }

    public long LatestVersion { get; set; }
}
