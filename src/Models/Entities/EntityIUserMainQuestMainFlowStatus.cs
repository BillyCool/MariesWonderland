using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserMainQuestMainFlowStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentMainQuestRouteId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public int HeadQuestSceneId { get; set; }

    public bool IsReachedLastQuestScene { get; set; }

    public long LatestVersion { get; set; }
}
