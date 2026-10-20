using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserDeckNumber), nameof(UserDeckCharacterNumber))]
public class EntityIUserDeckLimitContentDeletedCharacter : IUserEntity
{
    public long UserId { get; set; }

    public int UserDeckNumber { get; set; }

    public int UserDeckCharacterNumber { get; set; }

    public int CostumeId { get; set; }

    public long LatestVersion { get; set; }
}
