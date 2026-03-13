using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLimitContentDeckRestrictionTargetTable : TableBase<EntityMEventQuestLimitContentDeckRestrictionTarget>
{
    private readonly Func<EntityMEventQuestLimitContentDeckRestrictionTarget, (int, LimitContentDeckRestrictionType)> primaryIndexSelector;
    private readonly Func<EntityMEventQuestLimitContentDeckRestrictionTarget, int> secondaryIndexSelector;

    public EntityMEventQuestLimitContentDeckRestrictionTargetTable(EntityMEventQuestLimitContentDeckRestrictionTarget[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestLimitContentDeckRestrictionTargetId, element.LimitContentDeckRestrictionType);
        secondaryIndexSelector = element => element.EventQuestLimitContentDeckRestrictionTargetId;
    }

    public RangeView<EntityMEventQuestLimitContentDeckRestrictionTarget> FindByEventQuestLimitContentDeckRestrictionTargetId(int key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
