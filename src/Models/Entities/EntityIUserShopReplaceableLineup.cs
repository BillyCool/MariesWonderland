using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserShopReplaceableLineup : IUserEntity
{
    public long UserId { get; set; }

    public int SlotNumber { get; set; }

    public int ShopItemId { get; set; }

    public long LatestVersion { get; set; }
}
