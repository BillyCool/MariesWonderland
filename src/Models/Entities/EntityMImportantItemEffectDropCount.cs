using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItemEffectDropCount))]
public class EntityMImportantItemEffectDropCount
{
    [Key(0)] public int ImportantItemEffectDropCountId { get; set; }

    [Key(1)] public int CountPermil { get; set; }

    [Key(2)] public int ImportantItemEffectTargetQuestGroupId { get; set; }

    [Key(3)] public int ImportantItemEffectTargetItemGroupId { get; set; }
}
