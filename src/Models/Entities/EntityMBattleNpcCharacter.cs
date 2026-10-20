using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacter))]
[PrimaryKey(nameof(BattleNpcId), nameof(CharacterId))]
public class EntityMBattleNpcCharacter
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterId { get; set; }

    [Key(2)] public int Level { get; set; }

    [Key(3)] public int Exp { get; set; }
}
