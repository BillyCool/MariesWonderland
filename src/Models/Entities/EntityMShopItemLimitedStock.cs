using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItemLimitedStock))]
public class EntityMShopItemLimitedStock
{
    [Key(0)] public int ShopItemLimitedStockId { get; set; }

    [Key(1)] public int MaxCount { get; set; }

    [Key(2)] public AutoResetType ShopItemAutoResetType { get; set; }

    [Key(3)] public int ShopItemAutoResetPeriod { get; set; }
}
