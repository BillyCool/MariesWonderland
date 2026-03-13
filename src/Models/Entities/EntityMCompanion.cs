using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCompanion
{
    public int CompanionId { get; set; }

    public AttributeType AttributeType { get; set; }

    public int CompanionCategoryType { get; set; }

    public int CompanionBaseStatusId { get; set; }

    public int CompanionStatusCalculationId { get; set; }

    public int SkillId { get; set; }

    public int CompanionAbilityGroupId { get; set; }

    public int ActorId { get; set; }

    public int ActorSkeletonId { get; set; }

    public int AssetVariationId { get; set; }

    public int CharacterMoverBattleActorAiId { get; set; }
}
