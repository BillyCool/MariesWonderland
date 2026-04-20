using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardCondition))]
public class EntityMCharacterBoardCondition
{
    [Key(0)] public int CharacterBoardConditionGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public CharacterBoardConditionType CharacterBoardConditionType { get; set; }

    [Key(3)] public int CharacterBoardConditionDetailId { get; set; }

    [Key(4)] public int CharacterBoardConditionIgnoreId { get; set; }

    [Key(5)] public int ConditionValue { get; set; }
}
