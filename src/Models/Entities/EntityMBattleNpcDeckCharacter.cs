using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckCharacter
{
    public long BattleNpcId { get; set; }

    public string BattleNpcDeckCharacterUuid { get; set; }

    public string BattleNpcCostumeUuid { get; set; }

    public string MainBattleNpcWeaponUuid { get; set; }

    public string BattleNpcCompanionUuid { get; set; }

    public int Power { get; set; }

    public string BattleNpcThoughtUuid { get; set; }
}
