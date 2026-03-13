using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckPartsGroup
{
    public long BattleNpcId { get; set; }

    public string BattleNpcDeckCharacterUuid { get; set; }

    public string BattleNpcPartsUuid { get; set; }

    public int SortOrder { get; set; }
}
