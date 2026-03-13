using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionAttributeDamageCorrection
{
    public int SkillBehaviourActionId { get; set; }

    public AttributeType AttributeType { get; set; }

    public CorrectionTargetDamageType CorrectionTargetDamageType { get; set; }

    public int CorrectionValuePermil { get; set; }

    public DamageCorrectionOverlapType DamageCorrectionOverlapType { get; set; }

    public bool IsExcepting { get; set; }
}
