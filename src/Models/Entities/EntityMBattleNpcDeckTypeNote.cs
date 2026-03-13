using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckTypeNote
{
    public long BattleNpcId { get; set; }

    public DeckType DeckType { get; set; }

    public int MaxDeckPower { get; set; }
}
