using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardPanelReleaseRewardGroupTable : TableBase<EntityMCharacterBoardPanelReleaseRewardGroup>
{
    private readonly Func<EntityMCharacterBoardPanelReleaseRewardGroup, (int, PossessionType, int)> primaryIndexSelector;

    public EntityMCharacterBoardPanelReleaseRewardGroupTable(EntityMCharacterBoardPanelReleaseRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterBoardPanelReleaseRewardGroupId, element.PossessionType, element.PossessionId);
    }
}
