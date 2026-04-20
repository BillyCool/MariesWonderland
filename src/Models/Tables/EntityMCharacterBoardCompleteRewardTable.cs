using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardCompleteRewardTable : TableBase<EntityMCharacterBoardCompleteReward>
{
    private readonly Func<EntityMCharacterBoardCompleteReward, int> primaryIndexSelector;

    public EntityMCharacterBoardCompleteRewardTable(EntityMCharacterBoardCompleteReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CharacterBoardCompleteRewardId;
    }
}
