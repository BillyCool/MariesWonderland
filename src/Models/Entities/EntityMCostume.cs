using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostume))]
public class EntityMCostume
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int CharacterId { get; set; }

    [Key(2)] public int ActorId { get; set; }

    [Key(3)] public CostumeAssetCategoryType CostumeAssetCategoryType { get; set; }

    [Key(4)] public int ActorSkeletonId { get; set; }

    [Key(5)] public int AssetVariationId { get; set; }

    [Key(6)] public WeaponType SkillfulWeaponType { get; set; }

    [Key(7)] public RarityType RarityType { get; set; }

    [Key(8)] public int CostumeBaseStatusId { get; set; }

    [Key(9)] public int CostumeStatusCalculationId { get; set; }

    [Key(10)] public int CostumeLimitBreakMaterialGroupId { get; set; }

    [Key(11)] public int CostumeAbilityGroupId { get; set; }

    [Key(12)] public int CostumeActiveSkillGroupId { get; set; }

    [Key(13)] public int CounterSkillDetailId { get; set; }

    [Key(14)] public int CharacterMoverBattleActorAiId { get; set; }

    [Key(15)] public int CostumeDefaultSkillGroupId { get; set; }

    [Key(16)] public int CostumeLevelBonusId { get; set; }

    [Key(17)] public int DefaultActorSkillAiId { get; set; }

    [Key(18)] public int CostumeEmblemAssetId { get; set; }

    [Key(19)] public int BattleActorSkillAiGroupId { get; set; }
}
