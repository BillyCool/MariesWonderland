using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserExtraQuestProgressStatusTable : TableBase<EntityIUserExtraQuestProgressStatus>
{
    private readonly Func<EntityIUserExtraQuestProgressStatus, long> primaryIndexSelector;

    public EntityIUserExtraQuestProgressStatusTable(EntityIUserExtraQuestProgressStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserExtraQuestProgressStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
