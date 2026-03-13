using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntQuestScoreCoefficientTable : TableBase<EntityMBigHuntQuestScoreCoefficient>
{
    private readonly Func<EntityMBigHuntQuestScoreCoefficient, int> primaryIndexSelector;

    public EntityMBigHuntQuestScoreCoefficientTable(EntityMBigHuntQuestScoreCoefficient[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BigHuntQuestScoreCoefficientId;
    }

    public EntityMBigHuntQuestScoreCoefficient FindByBigHuntQuestScoreCoefficientId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
