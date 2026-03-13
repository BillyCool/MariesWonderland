using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterRebirth
{
    public int CharacterId { get; set; }

    public int CharacterRebirthStepGroupId { get; set; }

    public int SortOrder { get; set; }

    public CharacterAssignmentType CharacterAssignmentType { get; set; }
}
