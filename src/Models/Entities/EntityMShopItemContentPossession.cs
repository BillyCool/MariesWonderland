using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItemContentPossession))]
public class EntityMShopItemContentPossession
{
    [Key(0)] public int ShopItemId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int SortOrder { get; set; }

    [Key(4)] public int Count { get; set; }
}
