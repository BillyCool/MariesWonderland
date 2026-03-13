using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckCharacterType
{
    public long BattleNpcId { get; set; }

    public string BattleNpcDeckCharacterUuid { get; set; }

    public BattleEnemyType BattleEnemyType { get; set; }
}
