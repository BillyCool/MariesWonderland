using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMOmikujiTable : TableBase<EntityMOmikuji>
{
    private readonly Func<EntityMOmikuji, int> primaryIndexSelector;

    public EntityMOmikujiTable(EntityMOmikuji[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.OmikujiId;
    }
}
