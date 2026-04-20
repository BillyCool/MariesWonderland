using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestTable : TableBase<EntityMQuest>
{
    private readonly Func<EntityMQuest, int> primaryIndexSelector;

    public EntityMQuestTable(EntityMQuest[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestId;
    }

    public EntityMQuest FindByQuestId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByQuestId(int key, out EntityMQuest result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
