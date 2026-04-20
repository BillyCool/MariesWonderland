using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTipGroupSelection))]
public class EntityMTipGroupSelection
{
    [Key(0)] public int TipGroupId { get; set; }

    [Key(1)] public int TipId { get; set; }

    [Key(2)] public int EncounterRatePermil { get; set; }
}
