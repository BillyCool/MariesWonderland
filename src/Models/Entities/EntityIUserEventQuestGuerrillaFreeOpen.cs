using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserEventQuestGuerrillaFreeOpen : IUserEntity
{
    public long UserId { get; set; }

    public long StartDatetime { get; set; }

    public int OpenMinutes { get; set; }

    public int DailyOpenedCount { get; set; }

    public long LatestVersion { get; set; }
}
