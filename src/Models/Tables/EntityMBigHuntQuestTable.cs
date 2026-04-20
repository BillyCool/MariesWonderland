using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntQuestTable : TableBase<EntityMBigHuntQuest>
{
    private readonly Func<EntityMBigHuntQuest, int> primaryIndexSelector;

    public EntityMBigHuntQuestTable(EntityMBigHuntQuest[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BigHuntQuestId;
    }

    public EntityMBigHuntQuest FindByBigHuntQuestId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
