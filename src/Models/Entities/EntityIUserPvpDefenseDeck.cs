using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPvpDefenseDeck : IUserEntity
{
    public long UserId { get; set; }

    public int UserDeckNumber { get; set; }

    public long LatestVersion { get; set; }
}
