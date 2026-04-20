using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMTutorialConsumePossessionGroupTable : TableBase<EntityMTutorialConsumePossessionGroup>
{
    private readonly Func<EntityMTutorialConsumePossessionGroup, (TutorialType, PossessionType, int)> primaryIndexSelector;

    private readonly Func<EntityMTutorialConsumePossessionGroup, TutorialType> secondaryIndexSelector;

    public EntityMTutorialConsumePossessionGroupTable(EntityMTutorialConsumePossessionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.TutorialType, element.PossessionType, element.PossessionId);
        secondaryIndexSelector = element => element.TutorialType;
    }

    public RangeView<EntityMTutorialConsumePossessionGroup> FindByTutorialType(TutorialType key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<TutorialType>.Default, key);
}
