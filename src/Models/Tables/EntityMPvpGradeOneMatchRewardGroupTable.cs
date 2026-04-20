using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpGradeOneMatchRewardGroupTable : TableBase<EntityMPvpGradeOneMatchRewardGroup>
{
    private readonly Func<EntityMPvpGradeOneMatchRewardGroup, (int, int)> primaryIndexSelector;

    public EntityMPvpGradeOneMatchRewardGroupTable(EntityMPvpGradeOneMatchRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PvpGradeOneMatchRewardGroupId, element.PvpGradeOneMatchRewardId);
    }
}
