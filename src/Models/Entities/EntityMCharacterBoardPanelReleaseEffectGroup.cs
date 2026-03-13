using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoardPanelReleaseEffectGroup
{
    public int CharacterBoardPanelReleaseEffectGroupId { get; set; }

    public int SortOrder { get; set; }

    public CharacterBoardEffectType CharacterBoardEffectType { get; set; }

    public int CharacterBoardEffectId { get; set; }

    public int EffectValue { get; set; }
}
