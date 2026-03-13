using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCharacterRebirth
{
    public long BattleNpcId { get; set; }

    public int CharacterId { get; set; }

    public int RebirthCount { get; set; }
}
