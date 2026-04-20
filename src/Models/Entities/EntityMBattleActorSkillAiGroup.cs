using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleActorSkillAiGroup))]
public class EntityMBattleActorSkillAiGroup
{
    [Key(0)] public int BattleActorSkillAiGroupId { get; set; }

    [Key(1)] public int Priority { get; set; }

    [Key(2)] public BattleSchemeType BattleSchemeType { get; set; }

    [Key(3)] public bool IsPlayerSide { get; set; }

    [Key(4)] public SkillAiUnlockConditionValueType SkillAiUnlockConditionValueType { get; set; }

    [Key(5)] public int SkillAiUnlockConditionValue { get; set; }

    [Key(6)] public int BattleActorSkillAiId { get; set; }
}
