using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserTutorialProgressTable : TableBase<EntityIUserTutorialProgress>
{
    private readonly Func<EntityIUserTutorialProgress, (long, TutorialType)> primaryIndexSelector;

    public EntityIUserTutorialProgressTable(EntityIUserTutorialProgress[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.TutorialType);
    }

    public bool TryFindByUserIdAndTutorialType(ValueTuple<long, TutorialType> key, out EntityIUserTutorialProgress result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, TutorialType)>.Default, key, out result);
}
