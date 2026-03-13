using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusTable : TableBase<EntityMQuestBonus>
{
    private readonly Func<EntityMQuestBonus, int> primaryIndexSelector;

    public EntityMQuestBonusTable(EntityMQuestBonus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestBonusId;
    }

    public EntityMQuestBonus FindByQuestBonusId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
