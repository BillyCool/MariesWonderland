using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPvpWeeklyResultTable : TableBase<EntityIUserPvpWeeklyResult>
{
    private readonly Func<EntityIUserPvpWeeklyResult, (long, long)> primaryIndexSelector;

    public EntityIUserPvpWeeklyResultTable(EntityIUserPvpWeeklyResult[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.PvpWeeklyVersion);
    }
}
