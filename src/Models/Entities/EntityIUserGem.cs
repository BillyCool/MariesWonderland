using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserGem : IUserEntity
{
    public long UserId { get; set; }

    public int PaidGem { get; set; }

    public int FreeGem { get; set; }
}
