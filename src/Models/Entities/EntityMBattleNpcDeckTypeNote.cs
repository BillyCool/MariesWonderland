using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckTypeNote))]
[PrimaryKey(nameof(BattleNpcId), nameof(DeckType))]
public class EntityMBattleNpcDeckTypeNote
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public DeckType DeckType { get; set; }

    [Key(2)] public int MaxDeckPower { get; set; }
}
