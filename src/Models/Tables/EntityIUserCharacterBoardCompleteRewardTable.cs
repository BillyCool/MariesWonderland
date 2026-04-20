using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCharacterBoardCompleteRewardTable : TableBase<EntityIUserCharacterBoardCompleteReward>
{
    private readonly Func<EntityIUserCharacterBoardCompleteReward, (long, int)> primaryIndexSelector;

    public EntityIUserCharacterBoardCompleteRewardTable(EntityIUserCharacterBoardCompleteReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.CharacterBoardCompleteRewardId);
    }
}
