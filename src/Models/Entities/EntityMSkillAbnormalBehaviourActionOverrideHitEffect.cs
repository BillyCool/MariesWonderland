using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionOverrideHitEffect
{
    public int SkillAbnormalBehaviourActionId { get; set; }

    public int OverrideEffectId { get; set; }

    public int OverrideSeId { get; set; }

    public int Priority { get; set; }

    public bool DisablePlayHitVoice { get; set; }

    public bool PlayOnMiss { get; set; }

    public bool ForceRotateOnHit { get; set; }

    public int OverrideHitEffectConditionGroupId { get; set; }

    public int OverrideHitEffectConditionOperationType { get; set; }
}
