using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(ImportantItemId))]
public class EntityIUserImportantItem : IUserEntity
{
    public long UserId { get; set; }

    public int ImportantItemId { get; set; }

    public int Count { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
