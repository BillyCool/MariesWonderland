using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestDisplayEnemyThumbnailReplaceTable : TableBase<EntityMQuestDisplayEnemyThumbnailReplace>
{
    private readonly Func<EntityMQuestDisplayEnemyThumbnailReplace, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMQuestDisplayEnemyThumbnailReplace, int> secondaryIndexSelector;

    public EntityMQuestDisplayEnemyThumbnailReplaceTable(EntityMQuestDisplayEnemyThumbnailReplace[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestId, element.Priority);
        secondaryIndexSelector = element => element.QuestId;
    }

    public RangeView<EntityMQuestDisplayEnemyThumbnailReplace> FindByQuestId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
