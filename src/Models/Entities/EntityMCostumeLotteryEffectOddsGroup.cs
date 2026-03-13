using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeLotteryEffectOddsGroup
{
    public int CostumeLotteryEffectOddsGroupId { get; set; }

    public int OddsNumber { get; set; }

    public int Weight { get; set; }

    public CostumeLotteryEffectType CostumeLotteryEffectType { get; set; }

    public int CostumeLotteryEffectTargetId { get; set; }

    public RarityType RarityType { get; set; }
}
