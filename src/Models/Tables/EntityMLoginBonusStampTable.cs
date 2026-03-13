using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMLoginBonusStampTable : TableBase<EntityMLoginBonusStamp>
{
    private readonly Func<EntityMLoginBonusStamp, (int, int, int)> primaryIndexSelector;

    public EntityMLoginBonusStampTable(EntityMLoginBonusStamp[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.LoginBonusId, element.LowerPageNumber, element.StampNumber);
    }
}
