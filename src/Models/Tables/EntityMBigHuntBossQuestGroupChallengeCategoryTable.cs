using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntBossQuestGroupChallengeCategoryTable : TableBase<EntityMBigHuntBossQuestGroupChallengeCategory>
{
    private readonly Func<EntityMBigHuntBossQuestGroupChallengeCategory, (int, int)> primaryIndexSelector;

    public EntityMBigHuntBossQuestGroupChallengeCategoryTable(EntityMBigHuntBossQuestGroupChallengeCategory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BigHuntBossQuestGroupChallengeCategoryId, element.SortOrder);
    }
}
