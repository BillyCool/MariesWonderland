using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckTypeNote))]
public class EntityMBattleNpcDeckTypeNote
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public DeckType DeckType { get; set; }

    [Key(2)] public int MaxDeckPower { get; set; }
}
