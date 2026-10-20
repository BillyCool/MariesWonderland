using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CharacterId))]
public class EntityIUserCharacter : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterId { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }

    public long LatestVersion { get; set; }
}
