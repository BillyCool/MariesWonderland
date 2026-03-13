using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCostumeLevelBonusReleaseStatus
{
    public long BattleNpcId { get; set; }

    public int CostumeId { get; set; }

    public int LastReleasedBonusLevel { get; set; }

    public int ConfirmedBonusLevel { get; set; }
}
