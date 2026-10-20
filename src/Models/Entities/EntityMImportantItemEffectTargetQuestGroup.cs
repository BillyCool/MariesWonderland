using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItemEffectTargetQuestGroup))]
[PrimaryKey(nameof(ImportantItemEffectTargetQuestGroupId), nameof(TargetIndex))]
public class EntityMImportantItemEffectTargetQuestGroup
{
    [Key(0)] public int ImportantItemEffectTargetQuestGroupId { get; set; }

    [Key(1)] public int TargetIndex { get; set; }

    [Key(2)] public ImportantItemEffectTargetQuestGroupType ImportantItemEffectTargetQuestGroupType { get; set; }

    [Key(3)] public int TargetValue { get; set; }
}
