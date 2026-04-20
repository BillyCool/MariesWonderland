using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionAbnormalResistance))]
public class EntityMSkillAbnormalBehaviourActionAbnormalResistance
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public AbnormalResistancePolarityType AbnormalResistancePolarityType { get; set; }

    [Key(2)] public int AbnormalResistanceSkillAbnormalTypeId { get; set; }

    [Key(3)] public int BlockProbabilityPermil { get; set; }
}
