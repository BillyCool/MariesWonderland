using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopDisplayPrice))]
public class EntityMShopDisplayPrice
{
    [Key(0)] public PriceType PriceType { get; set; }

    [Key(1)] public int PriceId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
