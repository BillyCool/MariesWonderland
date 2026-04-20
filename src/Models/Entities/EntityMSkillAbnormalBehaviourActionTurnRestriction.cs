using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionTurnRestriction))]
public class EntityMSkillAbnormalBehaviourActionTurnRestriction
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int TurnRestrictionProbabilityPermil { get; set; }

    [Key(2)] public AbnormalBehaviourTurnRestrictionSkillType AbnormalBehaviourTurnRestrictionSkillType { get; set; }
}
