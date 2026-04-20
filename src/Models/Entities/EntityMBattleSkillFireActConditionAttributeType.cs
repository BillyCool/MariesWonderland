using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillFireActConditionAttributeType))]
public class EntityMBattleSkillFireActConditionAttributeType
{
    [Key(0)] public int BattleSkillFireActConditionId { get; set; }

    [Key(1)] public AttributeType AttributeType { get; set; }
}
