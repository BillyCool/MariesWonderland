using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetimeBehaviourGroup))]
[PrimaryKey(nameof(SkillAbnormalLifetimeBehaviourGroupId), nameof(AbnormalLifetimeBehaviourIndex))]
public class EntityMSkillAbnormalLifetimeBehaviourGroup
{
    [Key(0)] public int SkillAbnormalLifetimeBehaviourGroupId { get; set; }

    [Key(1)] public int AbnormalLifetimeBehaviourIndex { get; set; }

    [Key(2)] public AbnormalLifetimeMethodType AbnormalLifetimeMethodType { get; set; }

    [Key(3)] public int SkillAbnormalLifetimeBehaviourId { get; set; }
}
