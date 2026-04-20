using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleBigHuntKnockDownGaugeValueConfigGroup))]
public class EntityMBattleBigHuntKnockDownGaugeValueConfigGroup
{
    [Key(0)] public int KnockDownGaugeValueConfigGroupId { get; set; }

    [Key(1)] public int ActiveSkillHitCount { get; set; }

    [Key(2)] public int DamageValueLowerLimit { get; set; }

    [Key(3)] public int GaugeValueLowerLimit { get; set; }

    [Key(4)] public int CorrectionRatioPermil { get; set; }
}
