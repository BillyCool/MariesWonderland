using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCharacterBoardCompleteReward : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterBoardCompleteRewardId { get; set; }

    public long LatestVersion { get; set; }
}
