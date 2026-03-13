using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleBigHuntKnockDownGaugeValueConfigGroup
{
    public int KnockDownGaugeValueConfigGroupId { get; set; }

    public int ActiveSkillHitCount { get; set; }

    public int DamageValueLowerLimit { get; set; }

    public int GaugeValueLowerLimit { get; set; }

    public int CorrectionRatioPermil { get; set; }
}
