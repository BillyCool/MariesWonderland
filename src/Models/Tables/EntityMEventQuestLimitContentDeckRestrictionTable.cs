using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLimitContentDeckRestrictionTable : TableBase<EntityMEventQuestLimitContentDeckRestriction>
{
    private readonly Func<EntityMEventQuestLimitContentDeckRestriction, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMEventQuestLimitContentDeckRestriction, int> secondaryIndexSelector;

    public EntityMEventQuestLimitContentDeckRestrictionTable(EntityMEventQuestLimitContentDeckRestriction[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestLimitContentDeckRestrictionId, element.GroupIndex);
        secondaryIndexSelector = element => element.EventQuestLimitContentDeckRestrictionId;
    }

    public RangeView<EntityMEventQuestLimitContentDeckRestriction> FindByEventQuestLimitContentDeckRestrictionId(int key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
