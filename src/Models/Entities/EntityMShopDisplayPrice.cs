using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopDisplayPrice))]
[PrimaryKey(nameof(PriceType), nameof(PriceId))]
public class EntityMShopDisplayPrice
{
    [Key(0)] public PriceType PriceType { get; set; }

    [Key(1)] public int PriceId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
