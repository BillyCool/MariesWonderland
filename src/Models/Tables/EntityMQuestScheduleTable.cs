using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestScheduleTable : TableBase<EntityMQuestSchedule>
{
    private readonly Func<EntityMQuestSchedule, int> primaryIndexSelector;

    public EntityMQuestScheduleTable(EntityMQuestSchedule[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestScheduleId;
    }

    public EntityMQuestSchedule FindByQuestScheduleId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByQuestScheduleId(int key, out EntityMQuestSchedule result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
