using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(EventQuestChapterId))]
public class EntityIUserEventQuestLabyrinthSeason : IUserEntity
{
    public long UserId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int LastJoinSeasonNumber { get; set; }

    public int LastSeasonRewardReceivedSeasonNumber { get; set; }

    public long LatestVersion { get; set; }
}
