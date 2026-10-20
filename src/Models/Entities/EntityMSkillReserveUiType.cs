using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillReserveUiType))]
[PrimaryKey(nameof(SkillDetailId))]
public class EntityMSkillReserveUiType
{
    [Key(0)] public int SkillDetailId { get; set; }

    [Key(1)] public int SkillReserveUiTypeId { get; set; }
}
