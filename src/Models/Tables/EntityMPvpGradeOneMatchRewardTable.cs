using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpGradeOneMatchRewardTable : TableBase<EntityMPvpGradeOneMatchReward>
{
    private readonly Func<EntityMPvpGradeOneMatchReward, (int, int)> primaryIndexSelector;

    public EntityMPvpGradeOneMatchRewardTable(EntityMPvpGradeOneMatchReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PvpGradeOneMatchRewardId, element.PvpRewardId);
    }
}
