using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestDeckMultiRestrictionGroupTable : TableBase<EntityMQuestDeckMultiRestrictionGroup>
{
    private readonly Func<EntityMQuestDeckMultiRestrictionGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMQuestDeckMultiRestrictionGroup, int> secondaryIndexSelector;

    public EntityMQuestDeckMultiRestrictionGroupTable(EntityMQuestDeckMultiRestrictionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestDeckMultiRestrictionGroupId, element.GroupIndex);
        secondaryIndexSelector = element => (element.QuestDeckMultiRestrictionGroupId);
    }

    public RangeView<EntityMQuestDeckMultiRestrictionGroup> FindByQuestDeckMultiRestrictionGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
