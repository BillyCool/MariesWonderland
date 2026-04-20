using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanion))]
public class EntityMCompanion
{
    [Key(0)] public int CompanionId { get; set; }

    [Key(1)] public AttributeType AttributeType { get; set; }

    [Key(2)] public int CompanionCategoryType { get; set; }

    [Key(3)] public int CompanionBaseStatusId { get; set; }

    [Key(4)] public int CompanionStatusCalculationId { get; set; }

    [Key(5)] public int SkillId { get; set; }

    [Key(6)] public int CompanionAbilityGroupId { get; set; }

    [Key(7)] public int ActorId { get; set; }

    [Key(8)] public int ActorSkeletonId { get; set; }

    [Key(9)] public int AssetVariationId { get; set; }

    [Key(10)] public int CharacterMoverBattleActorAiId { get; set; }
}
