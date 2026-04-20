using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPremiumItem))]
public class EntityMPremiumItem
{
    [Key(0)] public int PremiumItemId { get; set; }

    [Key(1)] public int PremiumItemType { get; set; }

    [Key(2)] public long StartDatetime { get; set; }

    [Key(3)] public long EndDatetime { get; set; }
}
