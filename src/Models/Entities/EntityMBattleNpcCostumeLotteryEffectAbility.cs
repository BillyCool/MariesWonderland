using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCostumeLotteryEffectAbility
{
    public long BattleNpcId { get; set; }

    public string BattleNpcCostumeUuid { get; set; }

    public int SlotNumber { get; set; }

    public int AbilityId { get; set; }

    public int AbilityLevel { get; set; }
}
