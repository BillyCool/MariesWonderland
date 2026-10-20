using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserPartsPresetTagNumber))]
public class EntityIUserPartsPresetTag : IUserEntity
{
    public long UserId { get; set; }

    public int UserPartsPresetTagNumber { get; set; }

    public string Name { get; set; }

    public long LatestVersion { get; set; }
}
