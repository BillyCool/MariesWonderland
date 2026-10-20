using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionHitRatioDown))]
[PrimaryKey(nameof(SkillAbnormalBehaviourActionId))]
public class EntityMSkillAbnormalBehaviourActionHitRatioDown
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public int Value { get; set; }
}
