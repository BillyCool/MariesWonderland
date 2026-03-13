using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeBaseStatusTable : TableBase<EntityMCostumeBaseStatus>
{
    private readonly Func<EntityMCostumeBaseStatus, int> primaryIndexSelector;

    public EntityMCostumeBaseStatusTable(EntityMCostumeBaseStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeBaseStatusId;
    }

    public EntityMCostumeBaseStatus FindByCostumeBaseStatusId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
