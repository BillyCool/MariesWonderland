using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(ShopItemId))]
public class EntityIUserShopItem : IUserEntity
{
    public long UserId { get; set; }

    public int ShopItemId { get; set; }

    public int BoughtCount { get; set; }

    public long LatestBoughtCountChangedDatetime { get; set; }

    public long LatestVersion { get; set; }
}
