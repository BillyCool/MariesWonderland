using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCompanion
{
    public long BattleNpcId { get; set; }

    public string BattleNpcCompanionUuid { get; set; }

    public int CompanionId { get; set; }

    public int HeadupDisplayViewId { get; set; }

    public int Level { get; set; }

    public long AcquisitionDatetime { get; set; }
}
