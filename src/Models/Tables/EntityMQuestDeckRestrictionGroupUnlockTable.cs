using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestDeckRestrictionGroupUnlockTable : TableBase<EntityMQuestDeckRestrictionGroupUnlock>
{
    private readonly Func<EntityMQuestDeckRestrictionGroupUnlock, int> primaryIndexSelector;

    public EntityMQuestDeckRestrictionGroupUnlockTable(EntityMQuestDeckRestrictionGroupUnlock[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestDeckRestrictionGroupId;
    }

    public EntityMQuestDeckRestrictionGroupUnlock FindByQuestDeckRestrictionGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
