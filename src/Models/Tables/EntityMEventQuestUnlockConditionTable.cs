using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestUnlockConditionTable : TableBase<EntityMEventQuestUnlockCondition>
{
    private readonly Func<EntityMEventQuestUnlockCondition, (EventQuestType, int, int)> primaryIndexSelector;
    private readonly Func<EntityMEventQuestUnlockCondition, (EventQuestType, UnlockConditionType)> secondaryIndexSelector;

    public EntityMEventQuestUnlockConditionTable(EntityMEventQuestUnlockCondition[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestType, element.CharacterId, element.QuestId);
        secondaryIndexSelector = element => (element.EventQuestType, element.UnlockConditionType);
    }

    public EntityMEventQuestUnlockCondition FindByEventQuestTypeAndCharacterIdAndQuestId(ValueTuple<EventQuestType, int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(EventQuestType, int, int)>.Default, key);

    public RangeView<EntityMEventQuestUnlockCondition> FindByEventQuestTypeAndUnlockConditionType(ValueTuple<EventQuestType, UnlockConditionType> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(EventQuestType, UnlockConditionType)>.Default, key);
}
