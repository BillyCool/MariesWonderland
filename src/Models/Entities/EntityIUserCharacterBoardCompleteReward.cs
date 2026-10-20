using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CharacterBoardCompleteRewardId))]
public class EntityIUserCharacterBoardCompleteReward : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterBoardCompleteRewardId { get; set; }

    public long LatestVersion { get; set; }
}
