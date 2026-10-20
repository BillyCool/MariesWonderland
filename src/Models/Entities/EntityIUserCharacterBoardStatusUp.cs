using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CharacterId), nameof(StatusCalculationType))]
public class EntityIUserCharacterBoardStatusUp : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterId { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int Hp { get; set; }

    public int Attack { get; set; }

    public int Vitality { get; set; }

    public int Agility { get; set; }

    public int CriticalRatio { get; set; }

    public int CriticalAttack { get; set; }

    public long LatestVersion { get; set; }
}
