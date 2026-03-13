using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntWeeklyAttributeScoreRewardGroupScheduleTable : TableBase<EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule>
{
    private readonly Func<EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule, (int, AttributeType, int)> primaryIndexSelector;

    public EntityMBigHuntWeeklyAttributeScoreRewardGroupScheduleTable(EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BigHuntWeeklyAttributeScoreRewardGroupScheduleId, element.AttributeType, element.GroupIndex);
    }
}
