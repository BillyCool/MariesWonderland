using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponConsumeExchangeConsumableItemGroup))]
public class EntityMWeaponConsumeExchangeConsumableItemGroup
{
    [Key(0)] public int WeaponId { get; set; }

    [Key(1)] public int ConsumableItemId { get; set; }

    [Key(2)] public int Count { get; set; }
}
