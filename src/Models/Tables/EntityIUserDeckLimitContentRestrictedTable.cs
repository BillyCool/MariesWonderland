using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserDeckLimitContentRestrictedTable : TableBase<EntityIUserDeckLimitContentRestricted>
{
    private readonly Func<EntityIUserDeckLimitContentRestricted, (long, int, int, string)> primaryIndexSelector;
    private readonly Func<EntityIUserDeckLimitContentRestricted, (int, PossessionType)> secondaryIndexSelector;

    public EntityIUserDeckLimitContentRestrictedTable(EntityIUserDeckLimitContentRestricted[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.EventQuestChapterId, element.QuestId, element.DeckRestrictedUuid);
        secondaryIndexSelector = element => (element.EventQuestChapterId, element.PossessionType);
    }

    public RangeView<EntityIUserDeckLimitContentRestricted> FindByEventQuestChapterIdAndPossessionType(ValueTuple<int, PossessionType> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(int, PossessionType)>.Default, key);
}
