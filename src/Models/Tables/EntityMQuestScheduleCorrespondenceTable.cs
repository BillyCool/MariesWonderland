using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestScheduleCorrespondenceTable : TableBase<EntityMQuestScheduleCorrespondence>
{
    private readonly Func<EntityMQuestScheduleCorrespondence, (int, int)> primaryIndexSelector;

    public EntityMQuestScheduleCorrespondenceTable(EntityMQuestScheduleCorrespondence[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestId, element.QuestScheduleId);
    }

    public RangeView<EntityMQuestScheduleCorrespondence> FindRangeByQuestIdAndQuestScheduleId(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
