using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserQuestTable : TableBase<EntityIUserQuest>
{
    private readonly Func<EntityIUserQuest, (long, int)> primaryIndexSelector;

    public EntityIUserQuestTable(EntityIUserQuest[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.QuestId);
    }

    public EntityIUserQuest FindByUserIdAndQuestId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndQuestId(ValueTuple<long, int> key, out EntityIUserQuest result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
