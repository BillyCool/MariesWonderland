using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLabyrinthMobTable : TableBase<EntityMEventQuestLabyrinthMob>
{
    private readonly Func<EntityMEventQuestLabyrinthMob, int> primaryIndexSelector;

    public EntityMEventQuestLabyrinthMobTable(EntityMEventQuestLabyrinthMob[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EventQuestLabyrinthMobId;
    }

    public bool TryFindByEventQuestLabyrinthMobId(int key, out EntityMEventQuestLabyrinthMob result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
