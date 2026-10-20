using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserGem : IUserEntity
{
    public long UserId { get; set; }

    public int PaidGem { get; set; }

    public int FreeGem { get; set; }
}
