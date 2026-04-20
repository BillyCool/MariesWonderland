using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusTermGroupTable : TableBase<EntityMQuestBonusTermGroup>
{
    private readonly Func<EntityMQuestBonusTermGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMQuestBonusTermGroup, int> secondaryIndexSelector;

    public EntityMQuestBonusTermGroupTable(EntityMQuestBonusTermGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestBonusTermGroupId, element.SortOrder);
        secondaryIndexSelector = element => element.QuestBonusTermGroupId;
    }

    public RangeView<EntityMQuestBonusTermGroup> FindByQuestBonusTermGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
