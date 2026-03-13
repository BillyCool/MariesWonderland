using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeck
{
    public long BattleNpcId { get; set; }

    public DeckType DeckType { get; set; }

    public int BattleNpcDeckNumber { get; set; }

    public string BattleNpcDeckCharacterUuid01 { get; set; }

    public string BattleNpcDeckCharacterUuid02 { get; set; }

    public string BattleNpcDeckCharacterUuid03 { get; set; }

    public string Name { get; set; }

    public int Power { get; set; }
}
