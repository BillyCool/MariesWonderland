using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMTutorialUnlockConditionTable : TableBase<EntityMTutorialUnlockCondition>
{
    private readonly Func<EntityMTutorialUnlockCondition, TutorialType> primaryIndexSelector;

    public EntityMTutorialUnlockConditionTable(EntityMTutorialUnlockCondition[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.TutorialType;
    }

    public EntityMTutorialUnlockCondition FindByTutorialType(TutorialType key) => FindUniqueCore(data, primaryIndexSelector, Comparer<TutorialType>.Default, key);

    public bool TryFindByTutorialType(TutorialType key, out EntityMTutorialUnlockCondition result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<TutorialType>.Default, key, out result);
}
