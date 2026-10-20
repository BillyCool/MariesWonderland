using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserDeckCharacterUuid), nameof(UserPartsUuid))]
public class EntityIUserDeckPartsGroup : IUserEntity
{
    public long UserId { get; set; }

    public string UserDeckCharacterUuid { get; set; }

    public string UserPartsUuid { get; set; }

    public int SortOrder { get; set; }

    public long LatestVersion { get; set; }
}
