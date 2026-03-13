using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMUserLevelTable : TableBase<EntityMUserLevel>
{
    private readonly Func<EntityMUserLevel, int> primaryIndexSelector;

    public EntityMUserLevelTable(EntityMUserLevel[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserLevel;
    }

    public EntityMUserLevel FindByUserLevel(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
