using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionBaseStatusTable : TableBase<EntityMCompanionBaseStatus>
{
    private readonly Func<EntityMCompanionBaseStatus, int> primaryIndexSelector;

    public EntityMCompanionBaseStatusTable(EntityMCompanionBaseStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CompanionBaseStatusId;
    }

    public EntityMCompanionBaseStatus FindByCompanionBaseStatusId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
