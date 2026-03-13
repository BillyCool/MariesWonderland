using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattle
{
    public int BattleId { get; set; }

    public long BattleNpcId { get; set; }

    public DeckType DeckType { get; set; }

    public int BattleNpcDeckNumber { get; set; }
}
