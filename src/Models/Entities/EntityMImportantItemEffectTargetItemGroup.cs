using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItemEffectTargetItemGroup))]
public class EntityMImportantItemEffectTargetItemGroup
{
    [Key(0)] public int ImportantItemEffectTargetItemGroupId { get; set; }

    [Key(1)] public int TargetIndex { get; set; }

    [Key(2)] public PossessionType PossessionType { get; set; }

    [Key(3)] public int PossessionId { get; set; }
}
