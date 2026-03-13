using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPartsGroupTable : TableBase<EntityMPartsGroup>
{
    private readonly Func<EntityMPartsGroup, int> primaryIndexSelector;

    public EntityMPartsGroupTable(EntityMPartsGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PartsGroupId;
    }

    public EntityMPartsGroup FindByPartsGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByPartsGroupId(int key, out EntityMPartsGroup result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
