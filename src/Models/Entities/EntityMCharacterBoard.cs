using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoard
{
    public int CharacterBoardId { get; set; }

    public int CharacterBoardGroupId { get; set; }

    public int CharacterBoardUnlockConditionGroupId { get; set; }

    public int ReleaseRank { get; set; }
}
