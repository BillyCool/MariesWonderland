using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CharacterId))]
public class EntityIUserCharacterRebirth : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterId { get; set; }

    public int RebirthCount { get; set; }

    public long LatestVersion { get; set; }
}
