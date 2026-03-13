using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterBoardGroup
{
    public int CharacterBoardGroupId { get; set; }

    public int CharacterBoardCategoryId { get; set; }

    public int SortOrder { get; set; }

    public CharacterBoardGroupType CharacterBoardGroupType { get; set; }

    public int TextAssetId { get; set; }
}
