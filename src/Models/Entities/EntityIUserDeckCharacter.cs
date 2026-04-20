using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDeckCharacter : IUserEntity
{
    public long UserId { get; set; }

    public string UserDeckCharacterUuid { get; set; }

    public string UserCostumeUuid { get; set; }

    public string MainUserWeaponUuid { get; set; }

    public string UserCompanionUuid { get; set; }

    public int Power { get; set; }

    public string UserThoughtUuid { get; set; }

    public long LatestVersion { get; set; }
}
