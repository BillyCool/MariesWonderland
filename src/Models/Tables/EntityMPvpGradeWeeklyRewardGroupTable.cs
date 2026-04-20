using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpGradeWeeklyRewardGroupTable : TableBase<EntityMPvpGradeWeeklyRewardGroup>
{
    private readonly Func<EntityMPvpGradeWeeklyRewardGroup, (int, int)> primaryIndexSelector;

    public EntityMPvpGradeWeeklyRewardGroupTable(EntityMPvpGradeWeeklyRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PvpGradeWeeklyRewardGroupId, element.PvpRewardId);
    }
}
