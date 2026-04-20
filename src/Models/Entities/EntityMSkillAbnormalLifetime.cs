using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetime))]
public class EntityMSkillAbnormalLifetime
{
    [Key(0)] public int SkillAbnormalLifetimeId { get; set; }

    [Key(1)] public int SkillAbnormalLifetimeBehaviourGroupId { get; set; }

    [Key(2)] public AbnormalLifetimeBehaviourConditionType AbnormalLifetimeBehaviourConditionType { get; set; }
}
