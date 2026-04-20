using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeck))]
public class EntityMBattleNpcDeck
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public DeckType DeckType { get; set; }

    [Key(2)] public int BattleNpcDeckNumber { get; set; }

    [Key(3)] public string BattleNpcDeckCharacterUuid01 { get; set; }

    [Key(4)] public string BattleNpcDeckCharacterUuid02 { get; set; }

    [Key(5)] public string BattleNpcDeckCharacterUuid03 { get; set; }

    [Key(6)] public string Name { get; set; }

    [Key(7)] public int Power { get; set; }
}
