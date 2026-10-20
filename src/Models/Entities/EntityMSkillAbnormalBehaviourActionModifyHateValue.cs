using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionModifyHateValue))]
[PrimaryKey(nameof(SkillAbnormalBehaviourActionId))]
public class EntityMSkillAbnormalBehaviourActionModifyHateValue
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public HateValueCalculationType HateValueCalculationType { get; set; }

    [Key(2)] public int ModifyValue { get; set; }
}
