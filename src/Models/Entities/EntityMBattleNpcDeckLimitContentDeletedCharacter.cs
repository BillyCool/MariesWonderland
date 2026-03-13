using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckLimitContentDeletedCharacter
{
    public long BattleNpcId { get; set; }

    public int BattleNpcDeckNumber { get; set; }

    public int BattleNpcDeckCharacterNumber { get; set; }

    public int CostumeId { get; set; }
}
