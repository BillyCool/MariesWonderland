using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillFireActConditionWeaponType))]
public class EntityMBattleSkillFireActConditionWeaponType
{
    [Key(0)] public int BattleSkillFireActConditionId { get; set; }

    [Key(1)] public WeaponType WeaponType { get; set; }
}
