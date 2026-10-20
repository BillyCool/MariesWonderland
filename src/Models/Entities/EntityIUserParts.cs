using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserPartsUuid))]
public class EntityIUserParts : IUserEntity
{
    public long UserId { get; set; }

    public string UserPartsUuid { get; set; }

    public int PartsId { get; set; }

    public int Level { get; set; }

    public int PartsStatusMainId { get; set; }

    public bool IsProtected { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
