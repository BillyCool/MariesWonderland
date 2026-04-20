using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserAppleTable : TableBase<EntityIUserApple>
{
    private readonly Func<EntityIUserApple, long> primaryIndexSelector;

    public EntityIUserAppleTable(EntityIUserApple[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserApple FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
