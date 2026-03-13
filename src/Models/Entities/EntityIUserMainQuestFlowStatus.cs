using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserMainQuestFlowStatus
{
    public long UserId { get; set; }

    public QuestFlowType CurrentQuestFlowType { get; set; }

    public long LatestVersion { get; set; }
}
