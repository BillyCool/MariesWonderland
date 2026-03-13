using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntBossQuestTable : TableBase<EntityMBigHuntBossQuest>
{
    private readonly Func<EntityMBigHuntBossQuest, int> primaryIndexSelector;

    public EntityMBigHuntBossQuestTable(EntityMBigHuntBossQuest[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BigHuntBossQuestId;
    }

    public EntityMBigHuntBossQuest FindByBigHuntBossQuestId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
