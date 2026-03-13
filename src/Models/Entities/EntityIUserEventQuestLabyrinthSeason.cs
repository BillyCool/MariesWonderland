using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserEventQuestLabyrinthSeason
{
    public long UserId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int LastJoinSeasonNumber { get; set; }

    public int LastSeasonRewardReceivedSeasonNumber { get; set; }

    public long LatestVersion { get; set; }
}
