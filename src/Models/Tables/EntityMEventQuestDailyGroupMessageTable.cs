using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestDailyGroupMessageTable : TableBase<EntityMEventQuestDailyGroupMessage>
{
    private readonly Func<EntityMEventQuestDailyGroupMessage, (int, int)> primaryIndexSelector;

    public EntityMEventQuestDailyGroupMessageTable(EntityMEventQuestDailyGroupMessage[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestDailyGroupMessageId, element.OddsNumber);
    }

    public bool TryFindByEventQuestDailyGroupMessageIdAndOddsNumber(ValueTuple<int, int> key, out EntityMEventQuestDailyGroupMessage result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);
}
