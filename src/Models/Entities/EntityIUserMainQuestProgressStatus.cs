using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserMainQuestProgressStatus
{
    public long UserId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public int HeadQuestSceneId { get; set; }

    public QuestFlowType CurrentQuestFlowType { get; set; }

    public long LatestVersion { get; set; }
}
