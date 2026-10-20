using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(EventQuestChapterId), nameof(QuestId), nameof(DeckRestrictedUuid))]
public class EntityIUserDeckLimitContentRestricted : IUserEntity
{
    public long UserId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int QuestId { get; set; }

    public string DeckRestrictedUuid { get; set; }

    public PossessionType PossessionType { get; set; }

    public string TargetUuid { get; set; }

    public long LatestVersion { get; set; }
}
