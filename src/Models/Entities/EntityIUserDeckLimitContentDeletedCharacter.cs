using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDeckLimitContentDeletedCharacter : IUserEntity
{
    public long UserId { get; set; }

    public int UserDeckNumber { get; set; }

    public int UserDeckCharacterNumber { get; set; }

    public int CostumeId { get; set; }

    public long LatestVersion { get; set; }
}
