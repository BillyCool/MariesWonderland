using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount))]
[PrimaryKey(nameof(SkillAbnormalLifetimeBehaviourId))]
public class EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount
{
    [Key(0)] public int SkillAbnormalLifetimeBehaviourId { get; set; }

    [Key(1)] public int ReceiveDamageCount { get; set; }
}
