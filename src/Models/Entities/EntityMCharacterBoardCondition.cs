using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoardCondition
{
    public int CharacterBoardConditionGroupId { get; set; }

    public int GroupIndex { get; set; }

    public CharacterBoardConditionType CharacterBoardConditionType { get; set; }

    public int CharacterBoardConditionDetailId { get; set; }

    public int CharacterBoardConditionIgnoreId { get; set; }

    public int ConditionValue { get; set; }
}
