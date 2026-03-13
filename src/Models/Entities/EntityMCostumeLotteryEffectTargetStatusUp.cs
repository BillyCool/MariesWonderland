using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeLotteryEffectTargetStatusUp
{
    public int CostumeLotteryEffectTargetStatusUpId { get; set; }

    public StatusKindType StatusKindType { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int EffectValue { get; set; }
}
