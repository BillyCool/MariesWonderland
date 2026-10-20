using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserCompanionUuid))]
public class EntityIUserCompanion : IUserEntity
{
    public long UserId { get; set; }

    public string UserCompanionUuid { get; set; }

    public int CompanionId { get; set; }

    public int HeadupDisplayViewId { get; set; }

    public int Level { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
