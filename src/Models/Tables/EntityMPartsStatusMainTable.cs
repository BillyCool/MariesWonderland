using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPartsStatusMainTable : TableBase<EntityMPartsStatusMain>
{
    private readonly Func<EntityMPartsStatusMain, int> primaryIndexSelector;

    public EntityMPartsStatusMainTable(EntityMPartsStatusMain[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PartsStatusMainId;
    }

    public EntityMPartsStatusMain FindByPartsStatusMainId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByPartsStatusMainId(int key, out EntityMPartsStatusMain result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
