using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserDeckCharacterUuid))]
public class EntityIUserDeckCharacterDressupCostume : IUserEntity
{
    public long UserId { get; set; }

    public string UserDeckCharacterUuid { get; set; }

    public int DressupCostumeId { get; set; }

    public long LatestVersion { get; set; }
}
