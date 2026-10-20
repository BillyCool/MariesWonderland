using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillFireActConditionAttributeType))]
[PrimaryKey(nameof(BattleSkillFireActConditionId))]
public class EntityMBattleSkillFireActConditionAttributeType
{
    [Key(0)] public int BattleSkillFireActConditionId { get; set; }

    [Key(1)] public AttributeType AttributeType { get; set; }
}
