using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserEventQuestGuerrillaFreeOpenTable : TableBase<EntityIUserEventQuestGuerrillaFreeOpen>
{
    private readonly Func<EntityIUserEventQuestGuerrillaFreeOpen, long> primaryIndexSelector;

    public EntityIUserEventQuestGuerrillaFreeOpenTable(EntityIUserEventQuestGuerrillaFreeOpen[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public bool TryFindByUserId(long key, out EntityIUserEventQuestGuerrillaFreeOpen result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key, out result);
}
