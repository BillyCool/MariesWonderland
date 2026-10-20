using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItemAdditionalContent))]
[PrimaryKey(nameof(ShopItemAdditionalContentId), nameof(PossessionType), nameof(PossessionId))]
public class EntityMShopItemAdditionalContent
{
    [Key(0)] public int ShopItemAdditionalContentId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int Count { get; set; }
}
