using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetimeBehaviourGroup))]
public class EntityMSkillAbnormalLifetimeBehaviourGroup
{
    [Key(0)] public int SkillAbnormalLifetimeBehaviourGroupId { get; set; }

    [Key(1)] public int AbnormalLifetimeBehaviourIndex { get; set; }

    [Key(2)] public AbnormalLifetimeMethodType AbnormalLifetimeMethodType { get; set; }

    [Key(3)] public int SkillAbnormalLifetimeBehaviourId { get; set; }
}
