using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormal))]
[PrimaryKey(nameof(SkillAbnormalId))]
public class EntityMSkillAbnormal
{
    [Key(0)] public int SkillAbnormalId { get; set; }

    [Key(1)] public int SkillAbnormalTypeId { get; set; }

    [Key(2)] public AbnormalPolarityType AbnormalPolarityType { get; set; }

    [Key(3)] public int SkillAbnormalLifetimeId { get; set; }

    [Key(4)] public int SkillAbnormalBehaviourGroupId { get; set; }
}
