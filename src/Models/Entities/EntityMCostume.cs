using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostume
{
    public int CostumeId { get; set; }

    public int CharacterId { get; set; }

    public int ActorId { get; set; }

    public CostumeAssetCategoryType CostumeAssetCategoryType { get; set; }

    public int ActorSkeletonId { get; set; }

    public int AssetVariationId { get; set; }

    public WeaponType SkillfulWeaponType { get; set; }

    public RarityType RarityType { get; set; }

    public int CostumeBaseStatusId { get; set; }

    public int CostumeStatusCalculationId { get; set; }

    public int CostumeLimitBreakMaterialGroupId { get; set; }

    public int CostumeAbilityGroupId { get; set; }

    public int CostumeActiveSkillGroupId { get; set; }

    public int CounterSkillDetailId { get; set; }

    public int CharacterMoverBattleActorAiId { get; set; }

    public int CostumeDefaultSkillGroupId { get; set; }

    public int CostumeLevelBonusId { get; set; }

    public int DefaultActorSkillAiId { get; set; }

    public int CostumeEmblemAssetId { get; set; }

    public int BattleActorSkillAiGroupId { get; set; }
}
