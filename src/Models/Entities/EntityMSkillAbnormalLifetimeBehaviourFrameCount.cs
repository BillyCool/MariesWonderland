using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalLifetimeBehaviourFrameCount))]
public class EntityMSkillAbnormalLifetimeBehaviourFrameCount
{
    [Key(0)] public int SkillAbnormalLifetimeBehaviourId { get; set; }

    [Key(1)] public int FrameCount { get; set; }
}
