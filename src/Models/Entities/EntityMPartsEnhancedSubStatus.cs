using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPartsEnhancedSubStatus
{
    public int PartsEnhancedId { get; set; }

    public int StatusIndex { get; set; }

    public int PartsStatusSubLotteryId { get; set; }

    public int Level { get; set; }

    public StatusKindType StatusKindType { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int FixedStatusChangeValue { get; set; }
}
