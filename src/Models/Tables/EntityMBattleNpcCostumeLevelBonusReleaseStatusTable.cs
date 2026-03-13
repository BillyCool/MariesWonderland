using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCostumeLevelBonusReleaseStatusTable : TableBase<EntityMBattleNpcCostumeLevelBonusReleaseStatus>
{
    private readonly Func<EntityMBattleNpcCostumeLevelBonusReleaseStatus, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcCostumeLevelBonusReleaseStatusTable(EntityMBattleNpcCostumeLevelBonusReleaseStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CostumeId);
    }
}
