using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntLinkTable : TableBase<EntityMBigHuntLink>
{
    private readonly Func<EntityMBigHuntLink, int> primaryIndexSelector;

    public EntityMBigHuntLinkTable(EntityMBigHuntLink[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BigHuntLinkId;
    }
}
