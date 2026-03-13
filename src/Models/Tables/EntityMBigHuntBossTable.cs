using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntBossTable : TableBase<EntityMBigHuntBoss>
{
    private readonly Func<EntityMBigHuntBoss, int> primaryIndexSelector;

    public EntityMBigHuntBossTable(EntityMBigHuntBoss[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BigHuntBossId;
    }

    public EntityMBigHuntBoss FindByBigHuntBossId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
