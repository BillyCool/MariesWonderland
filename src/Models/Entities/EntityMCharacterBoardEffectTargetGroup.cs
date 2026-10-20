using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardEffectTargetGroup))]
[PrimaryKey(nameof(CharacterBoardEffectTargetGroupId), nameof(GroupIndex))]
public class EntityMCharacterBoardEffectTargetGroup
{
    [Key(0)] public int CharacterBoardEffectTargetGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public CharacterBoardEffectType CharacterBoardEffectTargetType { get; set; }

    [Key(3)] public int TargetValue { get; set; }
}
