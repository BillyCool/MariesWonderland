using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserQuestAutoOrbit : IUserEntity
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
