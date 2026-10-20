using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoard))]
[PrimaryKey(nameof(CharacterBoardId))]
public class EntityMCharacterBoard
{
    [Key(0)] public int CharacterBoardId { get; set; }

    [Key(1)] public int CharacterBoardGroupId { get; set; }

    [Key(2)] public int CharacterBoardUnlockConditionGroupId { get; set; }

    [Key(3)] public int ReleaseRank { get; set; }
}
