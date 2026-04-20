using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleBigHuntDamageThresholdGroup))]
public class EntityMBattleBigHuntDamageThresholdGroup
{
    [Key(0)] public int KnockDownDamageThresholdGroupId { get; set; }

    [Key(1)] public int KnockDownDamageThresholdGroupOrder { get; set; }

    [Key(2)] public int KnockDownCumulativeDamageThreshold { get; set; }

    [Key(3)] public bool IsKnockDown { get; set; }

    [Key(4)] public int KnockDownDurationFrameCount { get; set; }

    [Key(5)] public int DamageRatio { get; set; }
}
