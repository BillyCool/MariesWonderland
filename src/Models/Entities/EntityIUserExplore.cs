using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserExplore
{
    public long UserId { get; set; }

    public bool IsUseExploreTicket { get; set; }

    public int PlayingExploreId { get; set; }

    public long LatestPlayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
