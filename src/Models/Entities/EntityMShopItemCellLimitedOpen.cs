using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItemCellLimitedOpen))]
public class EntityMShopItemCellLimitedOpen
{
    [Key(0)] public int ShopItemCellId { get; set; }

    [Key(1)] public int LimitedOpenId { get; set; }
}
