using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways))]
public class EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways
{
    [Key(0)] public int DamageMultiplyAbnormalDetailId { get; set; }

    [Key(1)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
