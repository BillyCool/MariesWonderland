using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMLoginBonusTable : TableBase<EntityMLoginBonus>
{
    private readonly Func<EntityMLoginBonus, int> primaryIndexSelector;

    public EntityMLoginBonusTable(EntityMLoginBonus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.LoginBonusId;
    }

    public EntityMLoginBonus FindByLoginBonusId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
