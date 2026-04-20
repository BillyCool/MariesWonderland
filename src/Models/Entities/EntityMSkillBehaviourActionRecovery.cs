using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionRecovery))]
public class EntityMSkillBehaviourActionRecovery
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int SkillPower { get; set; }

    [Key(2)] public int FixedRecoveryPoint { get; set; }

    [Key(3)] public int HpRatioRecoveryPointPermil { get; set; }

    [Key(4)] public int RecoveryPointMinValue { get; set; }

    [Key(5)] public int RecoveryPointMaxValue { get; set; }
}
