using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillDetail
{
    public int SkillDetailId { get; set; }

    public int SkillBehaviourGroupId { get; set; }

    public int NameSkillTextId { get; set; }

    public int SkillCooltimeValue { get; set; }

    public int SkillCooltimeBehaviourGroupId { get; set; }

    public int CasttimeFrameCount { get; set; }

    public int HitRatioPermil { get; set; }

    public int SkillRangeMilli { get; set; }

    public int SkillHitAssetCalculatorId { get; set; }

    public bool IsCounterApplicable { get; set; }

    public bool IsComboCalculationTarget { get; set; }

    public int SkillAssetCategoryId { get; set; }

    public int SkillAssetVariationId { get; set; }

    public int DescriptionSkillTextId { get; set; }

    public SkillActType SkillActType { get; set; }

    public int SkillHitCount { get; set; }

    public int SkillPowerBaseValue { get; set; }

    public PowerCalculationReferenceStatusType PowerCalculationReferenceStatusType { get; set; }

    public int PowerReferenceStatusGroupId { get; set; }

    public int SkillCasttimeId { get; set; }
}
