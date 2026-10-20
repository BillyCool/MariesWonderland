using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardConditionIgnore))]
[PrimaryKey(nameof(CharacterBoardConditionIgnoreId), nameof(IgnoreIndex))]
public class EntityMCharacterBoardConditionIgnore
{
    [Key(0)] public int CharacterBoardConditionIgnoreId { get; set; }

    [Key(1)] public int IgnoreIndex { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
