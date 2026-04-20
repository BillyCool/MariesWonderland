using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardAssignment))]
public class EntityMCharacterBoardAssignment
{
    [Key(0)] public int CharacterId { get; set; }

    [Key(1)] public int CharacterBoardCategoryId { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public CharacterBoardAssignmentType CharacterBoardAssignmentType { get; set; }
}
