using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetimeBehaviourActivateCount))]
[PrimaryKey(nameof(SkillAbnormalLifetimeBehaviourId))]
public class EntityMSkillAbnormalLifetimeBehaviourActivateCount
{
    [Key(0)] public int SkillAbnormalLifetimeBehaviourId { get; set; }

    [Key(1)] public int ActivateCount { get; set; }

    [Key(2)] public int AbnormalBehaviourIndex { get; set; }
}
