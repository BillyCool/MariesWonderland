using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItemEffect))]
public class EntityMImportantItemEffect
{
    [Key(0)] public int ImportantItemEffectId { get; set; }

    [Key(1)] public int ImportantItemEffectGroupingId { get; set; }

    [Key(2)] public int Priority { get; set; }

    [Key(3)] public int ImportantItemEffectType { get; set; }

    [Key(4)] public int ImportantItemEffectTargetId { get; set; }

    [Key(5)] public long StartDatetime { get; set; }

    [Key(6)] public long EndDatetime { get; set; }
}
