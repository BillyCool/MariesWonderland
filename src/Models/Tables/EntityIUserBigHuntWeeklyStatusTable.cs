using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserBigHuntWeeklyStatusTable : TableBase<EntityIUserBigHuntWeeklyStatus>
{
    private readonly Func<EntityIUserBigHuntWeeklyStatus, (long, long)> primaryIndexSelector;

    public EntityIUserBigHuntWeeklyStatusTable(EntityIUserBigHuntWeeklyStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.BigHuntWeeklyVersion);
    }
}
