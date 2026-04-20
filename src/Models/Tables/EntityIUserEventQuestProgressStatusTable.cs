using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserEventQuestProgressStatusTable : TableBase<EntityIUserEventQuestProgressStatus>
{
    private readonly Func<EntityIUserEventQuestProgressStatus, long> primaryIndexSelector;

    public EntityIUserEventQuestProgressStatusTable(EntityIUserEventQuestProgressStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserEventQuestProgressStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
