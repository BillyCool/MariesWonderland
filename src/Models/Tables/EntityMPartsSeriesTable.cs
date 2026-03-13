using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPartsSeriesTable : TableBase<EntityMPartsSeries>
{
    private readonly Func<EntityMPartsSeries, int> primaryIndexSelector;

    public EntityMPartsSeriesTable(EntityMPartsSeries[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PartsSeriesId;
    }

    public EntityMPartsSeries FindByPartsSeriesId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
