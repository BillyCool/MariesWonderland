using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCharacterBoardAbility
{
    public long BattleNpcId { get; set; }

    public int CharacterId { get; set; }

    public int AbilityId { get; set; }

    public int Level { get; set; }
}
