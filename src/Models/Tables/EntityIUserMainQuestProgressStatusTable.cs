using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMainQuestProgressStatusTable : TableBase<EntityIUserMainQuestProgressStatus>
{
    private readonly Func<EntityIUserMainQuestProgressStatus, long> primaryIndexSelector;

    public EntityIUserMainQuestProgressStatusTable(EntityIUserMainQuestProgressStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserMainQuestProgressStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
