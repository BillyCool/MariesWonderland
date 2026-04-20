using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillReserveUiType))]
public class EntityMSkillReserveUiType
{
    [Key(0)] public int SkillDetailId { get; set; }

    [Key(1)] public int SkillReserveUiTypeId { get; set; }
}
