using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSmartphoneChatGroupMessageTable : TableBase<EntityMSmartphoneChatGroupMessage>
{
    private readonly Func<EntityMSmartphoneChatGroupMessage, (int, int)> primaryIndexSelector;

    public EntityMSmartphoneChatGroupMessageTable(EntityMSmartphoneChatGroupMessage[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SmartphoneChatGroupId, element.SortOrder);
    }

    public RangeView<EntityMSmartphoneChatGroupMessage> FindRangeBySmartphoneChatGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
