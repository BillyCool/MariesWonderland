using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(SlotNumber))]
public class EntityIUserShopReplaceableLineup : IUserEntity
{
    public long UserId { get; set; }

    public int SlotNumber { get; set; }

    public int ShopItemId { get; set; }

    public long LatestVersion { get; set; }
}
