using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardCategory))]
public class EntityMCharacterBoardCategory
{
    [Key(0)] public int CharacterBoardCategoryId { get; set; }

    [Key(1)] public int SortOrder { get; set; }
}
