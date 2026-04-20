using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCharacter : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterId { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }

    public long LatestVersion { get; set; }
}
