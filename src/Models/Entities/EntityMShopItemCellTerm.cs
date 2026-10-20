using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItemCellTerm))]
[PrimaryKey(nameof(ShopItemCellTermId))]
public class EntityMShopItemCellTerm
{
    [Key(0)] public int ShopItemCellTermId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }
}
