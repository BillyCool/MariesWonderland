using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardCategory))]
[PrimaryKey(nameof(CharacterBoardCategoryId))]
public class EntityMCharacterBoardCategory
{
    [Key(0)] public int CharacterBoardCategoryId { get; set; }

    [Key(1)] public int SortOrder { get; set; }
}
