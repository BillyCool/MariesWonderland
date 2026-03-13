using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleRentalDeck
{
    public int BattleGroupId { get; set; }

    public long BattleNpcId { get; set; }

    public DeckType DeckType { get; set; }

    public int BattleNpcDeckNumber { get; set; }
}
