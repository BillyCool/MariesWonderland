using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserPortalCageStatus : IUserEntity
{
    public long UserId { get; set; }

    public bool IsCurrentProgress { get; set; }

    public long DropItemStartDatetime { get; set; }

    public int CurrentDropItemCount { get; set; }

    public long LatestVersion { get; set; }
}
