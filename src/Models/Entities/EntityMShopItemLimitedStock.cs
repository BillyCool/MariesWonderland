using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShopItemLimitedStock
{
    public int ShopItemLimitedStockId { get; set; }

    public int MaxCount { get; set; }

    public AutoResetType ShopItemAutoResetType { get; set; }

    public int ShopItemAutoResetPeriod { get; set; }
}
