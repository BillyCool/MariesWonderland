using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyDetailSkillfulWeaponType))]
public class EntityMSkillDamageMultiplyDetailSkillfulWeaponType
{
    [Key(0)] public int SkillDamageMultiplyDetailId { get; set; }

    [Key(1)] public DamageMultiplySkillfulWeaponConditionTargetType ConditionTargetType { get; set; }

    [Key(2)] public WeaponType WeaponType { get; set; }

    [Key(3)] public bool IsSkillfulMainWeapon { get; set; }

    [Key(4)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
