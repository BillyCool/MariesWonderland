using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardConditionGroup))]
[PrimaryKey(nameof(CharacterBoardConditionGroupId))]
public class EntityMCharacterBoardConditionGroup
{
    [Key(0)] public int CharacterBoardConditionGroupId { get; set; }

    [Key(1)] public ConditionOperationType ConditionOperationType { get; set; }
}
