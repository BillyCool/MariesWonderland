using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPvpWeeklyResult : IUserEntity
{
    public long UserId { get; set; }

    public long PvpWeeklyVersion { get; set; }

    public int PvpSeasonId { get; set; }

    public int GroupId { get; set; }

    public int FinalPoint { get; set; }

    public int FinalRank { get; set; }

    public long LatestVersion { get; set; }
}
