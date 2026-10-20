using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckLimitContentDeletedCharacter))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcDeckNumber), nameof(BattleNpcDeckCharacterNumber))]
public class EntityMBattleNpcDeckLimitContentDeletedCharacter
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int BattleNpcDeckNumber { get; set; }

    [Key(2)] public int BattleNpcDeckCharacterNumber { get; set; }

    [Key(3)] public int CostumeId { get; set; }
}
