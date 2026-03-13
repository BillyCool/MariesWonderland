using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleBigHuntPhaseGroup
{
    public int BattleBigHuntPhaseGroupId { get; set; }

    public int BattleBigHuntPhaseGroupOrder { get; set; }

    public int KnockDownDamageThresholdGroupId { get; set; }

    public int NormalPhaseFrameCount { get; set; }
}
