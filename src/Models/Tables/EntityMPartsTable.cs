using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPartsTable : TableBase<EntityMParts>
{
    private readonly Func<EntityMParts, int> primaryIndexSelector;

    public EntityMPartsTable(EntityMParts[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PartsId;
    }

    public EntityMParts FindByPartsId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
