using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserPartsUuid), nameof(StatusIndex))]
public class EntityIUserPartsStatusSub : IUserEntity
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
