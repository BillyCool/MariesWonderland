using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetimeBehaviourTurnCount))]
[PrimaryKey(nameof(SkillAbnormalLifetimeBehaviourId))]
public class EntityMSkillAbnormalLifetimeBehaviourTurnCount
{
    [Key(0)] public int SkillAbnormalLifetimeBehaviourId { get; set; }

    [Key(1)] public int TurnCount { get; set; }
}
