using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAbilityStatusTable : TableBase<EntityMAbilityStatus>
{
    private readonly Func<EntityMAbilityStatus, int> primaryIndexSelector;

    public EntityMAbilityStatusTable(EntityMAbilityStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AbilityStatusId;
    }

    public EntityMAbilityStatus FindByAbilityStatusId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
