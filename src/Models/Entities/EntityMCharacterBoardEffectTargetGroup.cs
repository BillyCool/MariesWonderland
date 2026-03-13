using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoardEffectTargetGroup
{
    public int CharacterBoardEffectTargetGroupId { get; set; }

    public int GroupIndex { get; set; }

    public CharacterBoardEffectType CharacterBoardEffectTargetType { get; set; }

    public int TargetValue { get; set; }
}
