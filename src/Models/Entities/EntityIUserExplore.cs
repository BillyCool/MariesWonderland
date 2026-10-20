using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserExplore : IUserEntity
{
    public long UserId { get; set; }

    public bool IsUseExploreTicket { get; set; }

    public int PlayingExploreId { get; set; }

    public long LatestPlayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
