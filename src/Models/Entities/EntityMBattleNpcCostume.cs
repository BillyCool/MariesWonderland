using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCostume
{
    public long BattleNpcId { get; set; }

    public string BattleNpcCostumeUuid { get; set; }

    public int CostumeId { get; set; }

    public int LimitBreakCount { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }

    public int HeadupDisplayViewId { get; set; }

    public long AcquisitionDatetime { get; set; }

    public int AwakenCount { get; set; }
}
