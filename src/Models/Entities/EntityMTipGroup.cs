using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTipGroup))]
public class EntityMTipGroup
{
    [Key(0)] public int TipGroupId { get; set; }

    [Key(1)] public int NameTextId { get; set; }
}
