using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPartsStatusSub
{
    public long UserId { get; set; }

    public string UserPartsUuid { get; set; }

    public int StatusIndex { get; set; }

    public int PartsStatusSubLotteryId { get; set; }

    public int Level { get; set; }

    public StatusKindType StatusKindType { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int StatusChangeValue { get; set; }

    public long LatestVersion { get; set; }
}
