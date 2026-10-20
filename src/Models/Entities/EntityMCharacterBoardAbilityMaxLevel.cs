using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardAbilityMaxLevel))]
[PrimaryKey(nameof(CharacterId), nameof(AbilityId))]
public class EntityMCharacterBoardAbilityMaxLevel
{
    [Key(0)] public int CharacterId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int MaxLevel { get; set; }
}
