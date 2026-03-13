using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserMainQuestMainFlowStatus
{
    public long UserId { get; set; }

    public int CurrentMainQuestRouteId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public int HeadQuestSceneId { get; set; }

    public bool IsReachedLastQuestScene { get; set; }

    public long LatestVersion { get; set; }
}
