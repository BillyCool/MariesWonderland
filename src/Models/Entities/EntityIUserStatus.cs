using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserStatus : IUserEntity
{
    public long UserId { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }

    public int StaminaMilliValue { get; set; }

    public long StaminaUpdateDatetime { get; set; }

    public long LatestVersion { get; set; }
}
