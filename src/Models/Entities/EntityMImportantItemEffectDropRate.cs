using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMImportantItemEffectDropRate
{
    public int ImportantItemEffectDropRateId { get; set; }

    public int RatePermil { get; set; }

    public int ImportantItemEffectTargetQuestGroupId { get; set; }

    public int ImportantItemEffectTargetItemGroupId { get; set; }
}
