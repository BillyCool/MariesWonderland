using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntScoreRewardGroupScheduleTable : TableBase<EntityMBigHuntScoreRewardGroupSchedule>
{
    private readonly Func<EntityMBigHuntScoreRewardGroupSchedule, (int, int)> primaryIndexSelector;

    public EntityMBigHuntScoreRewardGroupScheduleTable(EntityMBigHuntScoreRewardGroupSchedule[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BigHuntScoreRewardGroupScheduleId, element.GroupIndex);
    }
}
