using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillBehaviourHitDamageConfiguration))]
[PrimaryKey(nameof(SkillCategoryType), nameof(HitCount), nameof(HitIndexLowerLimit))]
public class EntityMBattleSkillBehaviourHitDamageConfiguration
{
    [Key(0)] public SkillCategoryType SkillCategoryType { get; set; }

    [Key(1)] public int HitCount { get; set; }

    [Key(2)] public int HitIndexLowerLimit { get; set; }

    [Key(3)] public int DamageCoefficientValuePermil { get; set; }
}
