using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMDokanTable : TableBase<EntityMDokan>
{
    private readonly Func<EntityMDokan, int> primaryIndexSelector;

    public EntityMDokanTable(EntityMDokan[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.DokanId;
    }
}
