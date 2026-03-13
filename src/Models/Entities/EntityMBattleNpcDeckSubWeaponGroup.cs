using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckSubWeaponGroup
{
    public long BattleNpcId { get; set; }

    public string BattleNpcDeckCharacterUuid { get; set; }

    public string BattleNpcWeaponUuid { get; set; }

    public int SortOrder { get; set; }
}
