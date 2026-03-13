using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCompanionTable : TableBase<EntityIUserCompanion>
{
    private readonly Func<EntityIUserCompanion, (long, string)> primaryIndexSelector;

    public EntityIUserCompanionTable(EntityIUserCompanion[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserCompanionUuid);
    }

    public EntityIUserCompanion FindByUserIdAndUserCompanionUuid(ValueTuple<long, string> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
