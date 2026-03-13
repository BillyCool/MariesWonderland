using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleBigHuntDamageThresholdGroup
{
    public int KnockDownDamageThresholdGroupId { get; set; }

    public int KnockDownDamageThresholdGroupOrder { get; set; }

    public int KnockDownCumulativeDamageThreshold { get; set; }

    public bool IsKnockDown { get; set; }

    public int KnockDownDurationFrameCount { get; set; }

    public int DamageRatio { get; set; }
}
