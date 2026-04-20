using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItemEffectTargetQuestGroup))]
public class EntityMImportantItemEffectTargetQuestGroup
{
    [Key(0)] public int ImportantItemEffectTargetQuestGroupId { get; set; }

    [Key(1)] public int TargetIndex { get; set; }

    [Key(2)] public ImportantItemEffectTargetQuestGroupType ImportantItemEffectTargetQuestGroupType { get; set; }

    [Key(3)] public int TargetValue { get; set; }
}
