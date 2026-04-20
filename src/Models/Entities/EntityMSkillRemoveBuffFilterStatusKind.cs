using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillRemoveBuffFilterStatusKind))]
public class EntityMSkillRemoveBuffFilterStatusKind
{
    [Key(0)] public int SkillRemoveBuffFilteringId { get; set; }

    [Key(1)] public int FilterIndex { get; set; }

    [Key(2)] public StatusKindType StatusKindType { get; set; }
}
