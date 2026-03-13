using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoardPanel
{
    public int CharacterBoardPanelId { get; set; }

    public int CharacterBoardId { get; set; }

    public int CharacterBoardPanelUnlockConditionGroupId { get; set; }

    public int CharacterBoardPanelReleasePossessionGroupId { get; set; }

    public int CharacterBoardPanelReleaseRewardGroupId { get; set; }

    public int CharacterBoardPanelReleaseEffectGroupId { get; set; }

    public int SortOrder { get; set; }

    public int ParentCharacterBoardPanelId { get; set; }

    public int PlaceIndex { get; set; }
}
