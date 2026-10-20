using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserMainQuestReplayFlowStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentHeadQuestSceneId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public long LatestVersion { get; set; }
}
