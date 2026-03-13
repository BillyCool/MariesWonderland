using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoardAssignment
{
    public int CharacterId { get; set; }

    public int CharacterBoardCategoryId { get; set; }

    public int SortOrder { get; set; }

    public CharacterBoardAssignmentType CharacterBoardAssignmentType { get; set; }
}
