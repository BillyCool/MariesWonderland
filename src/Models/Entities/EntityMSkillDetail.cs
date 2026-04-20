using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDetail))]
public class EntityMSkillDetail
{
    [Key(0)] public int SkillDetailId { get; set; }

    [Key(1)] public int SkillBehaviourGroupId { get; set; }

    [Key(2)] public int NameSkillTextId { get; set; }

    [Key(3)] public int SkillCooltimeValue { get; set; }

    [Key(4)] public int SkillCooltimeBehaviourGroupId { get; set; }

    [Key(5)] public int CasttimeFrameCount { get; set; }

    [Key(6)] public int HitRatioPermil { get; set; }

    [Key(7)] public int SkillRangeMilli { get; set; }

    [Key(8)] public int SkillHitAssetCalculatorId { get; set; }

    [Key(9)] public bool IsCounterApplicable { get; set; }

    [Key(10)] public bool IsComboCalculationTarget { get; set; }

    [Key(11)] public int SkillAssetCategoryId { get; set; }

    [Key(12)] public int SkillAssetVariationId { get; set; }

    [Key(13)] public int DescriptionSkillTextId { get; set; }

    [Key(14)] public SkillActType SkillActType { get; set; }

    [Key(15)] public int SkillHitCount { get; set; }

    [Key(16)] public int SkillPowerBaseValue { get; set; }

    [Key(17)] public PowerCalculationReferenceStatusType PowerCalculationReferenceStatusType { get; set; }

    [Key(18)] public int PowerReferenceStatusGroupId { get; set; }

    [Key(19)] public int SkillCasttimeId { get; set; }
}
