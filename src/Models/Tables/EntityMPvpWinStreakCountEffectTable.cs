using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpWinStreakCountEffectTable : TableBase<EntityMPvpWinStreakCountEffect>
{
    private readonly Func<EntityMPvpWinStreakCountEffect, int> primaryIndexSelector;

    public EntityMPvpWinStreakCountEffectTable(EntityMPvpWinStreakCountEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.WinStreakCount;
    }

    public EntityMPvpWinStreakCountEffect FindClosestByWinStreakCount(int key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<int>.Default, key, selectLower);
}
