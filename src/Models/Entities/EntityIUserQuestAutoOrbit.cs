using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserQuestAutoOrbit
{
    public long UserId { get; set; }

    public QuestType QuestType { get; set; }

    public int ChapterId { get; set; }

    public int QuestId { get; set; }

    public int MaxAutoOrbitCount { get; set; }

    public int ClearedAutoOrbitCount { get; set; }

    public long LastClearDatetime { get; set; }

    public long LatestVersion { get; set; }
}
