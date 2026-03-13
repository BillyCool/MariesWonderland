using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCostumeLotteryEffectPending
{
    public long BattleNpcId { get; set; }

    public string BattleNpcCostumeUuid { get; set; }

    public int SlotNumber { get; set; }

    public int OddsNumber { get; set; }
}
