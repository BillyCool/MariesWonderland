using MariesWonderland.MasterMemory;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardAbility))]
[PrimaryKey(nameof(CharacterBoardAbilityId))]
public class EntityMCharacterBoardAbility
{
    [Key(0)] public int CharacterBoardAbilityId { get; set; }

    [Key(1)] public int CharacterBoardEffectTargetGroupId { get; set; }

    [Key(2)] public int AbilityId { get; set; }
}
