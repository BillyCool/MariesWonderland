using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(PartsGroupId))]
public class EntityIUserPartsGroupNote : IUserEntity
{
    public long UserId { get; set; }

    public int PartsGroupId { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
