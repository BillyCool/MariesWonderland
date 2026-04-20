using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserLoginBonusTable : TableBase<EntityIUserLoginBonus>
{
    private readonly Func<EntityIUserLoginBonus, (long, int)> primaryIndexSelector;

    public EntityIUserLoginBonusTable(EntityIUserLoginBonus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.LoginBonusId);
    }

    public EntityIUserLoginBonus FindByUserIdAndLoginBonusId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndLoginBonusId(ValueTuple<long, int> key, out EntityIUserLoginBonus result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
