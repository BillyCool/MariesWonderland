using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntScoreRewardGroupTable : TableBase<EntityMBigHuntScoreRewardGroup>
{
    private readonly Func<EntityMBigHuntScoreRewardGroup, (int, long)> primaryIndexSelector;

    public EntityMBigHuntScoreRewardGroupTable(EntityMBigHuntScoreRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BigHuntScoreRewardGroupId, element.NecessaryScore);
    }
}
