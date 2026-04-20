using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardConditionGroup))]
public class EntityMCharacterBoardConditionGroup
{
    [Key(0)] public int CharacterBoardConditionGroupId { get; set; }

    [Key(1)] public ConditionOperationType ConditionOperationType { get; set; }
}
