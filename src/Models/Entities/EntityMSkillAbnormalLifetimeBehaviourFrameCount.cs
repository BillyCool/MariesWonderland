using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetimeBehaviourFrameCount))]
[PrimaryKey(nameof(SkillAbnormalLifetimeBehaviourId))]
public class EntityMSkillAbnormalLifetimeBehaviourFrameCount
{
    [Key(0)] public int SkillAbnormalLifetimeBehaviourId { get; set; }

    [Key(1)] public int FrameCount { get; set; }
}
