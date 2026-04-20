using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondenceTable : TableBase<EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondence>
{
    private readonly Func<EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondence, (int, int)> primaryIndexSelector;

    public EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondenceTable(EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondence[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestId, element.QuestScheduleId);
    }

    public RangeView<EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondence> FindRangeByQuestIdAndQuestScheduleId(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
