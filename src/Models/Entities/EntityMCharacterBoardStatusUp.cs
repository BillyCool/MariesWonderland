using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoardStatusUp
{
    public int CharacterBoardStatusUpId { get; set; }

    public CharacterBoardStatusUpType CharacterBoardStatusUpType { get; set; }

    public int CharacterBoardEffectTargetGroupId { get; set; }
}
