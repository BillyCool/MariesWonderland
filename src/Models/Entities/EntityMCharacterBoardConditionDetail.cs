using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardConditionDetail))]
public class EntityMCharacterBoardConditionDetail
{
    [Key(0)] public int CharacterBoardConditionDetailId { get; set; }

    [Key(1)] public int DetailIndex { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
