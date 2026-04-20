using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleRentalDeck))]
public class EntityMBattleRentalDeck
{
    [Key(0)] public int BattleGroupId { get; set; }

    [Key(1)] public long BattleNpcId { get; set; }

    [Key(2)] public DeckType DeckType { get; set; }

    [Key(3)] public int BattleNpcDeckNumber { get; set; }
}
