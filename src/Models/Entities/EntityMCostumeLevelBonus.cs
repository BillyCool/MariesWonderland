using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeLevelBonus
{
    public int CostumeLevelBonusId { get; set; }

    public int Level { get; set; }

    public CostumeLevelBonusType CostumeLevelBonusType { get; set; }

    public int EffectValue { get; set; }
}
