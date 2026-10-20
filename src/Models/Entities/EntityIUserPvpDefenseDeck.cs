using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserPvpDefenseDeck : IUserEntity
{
    public long UserId { get; set; }

    public int UserDeckNumber { get; set; }

    public long LatestVersion { get; set; }
}
