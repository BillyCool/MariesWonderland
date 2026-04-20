using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoard))]
public class EntityMCharacterBoard
{
    [Key(0)] public int CharacterBoardId { get; set; }

    [Key(1)] public int CharacterBoardGroupId { get; set; }

    [Key(2)] public int CharacterBoardUnlockConditionGroupId { get; set; }

    [Key(3)] public int ReleaseRank { get; set; }
}
