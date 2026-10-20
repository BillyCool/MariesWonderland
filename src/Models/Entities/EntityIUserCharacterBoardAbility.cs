using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CharacterId), nameof(AbilityId))]
public class EntityIUserCharacterBoardAbility : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterId { get; set; }

    public int AbilityId { get; set; }

    public int Level { get; set; }

    public long LatestVersion { get; set; }
}
