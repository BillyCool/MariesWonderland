using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardCompleteRewardGroupTable : TableBase<EntityMCharacterBoardCompleteRewardGroup>
{
    private readonly Func<EntityMCharacterBoardCompleteRewardGroup, (int, PossessionType, int)> primaryIndexSelector;

    public EntityMCharacterBoardCompleteRewardGroupTable(EntityMCharacterBoardCompleteRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterBoardCompleteRewardGroupId, element.PossessionType, element.PossessionId);
    }
}
