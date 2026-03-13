using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserEventQuestLabyrinthSeasonTable : TableBase<EntityIUserEventQuestLabyrinthSeason>
{
    private readonly Func<EntityIUserEventQuestLabyrinthSeason, (long, int)> primaryIndexSelector;

    public EntityIUserEventQuestLabyrinthSeasonTable(EntityIUserEventQuestLabyrinthSeason[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.EventQuestChapterId);
    }

    public EntityIUserEventQuestLabyrinthSeason FindByUserIdAndEventQuestChapterId(ValueTuple<long, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndEventQuestChapterId(ValueTuple<long, int> key, out EntityIUserEventQuestLabyrinthSeason result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);

    public RangeView<EntityIUserEventQuestLabyrinthSeason> FindRangeByUserIdAndEventQuestChapterId(ValueTuple<long, int> min, ValueTuple<long, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, min, max, ascendant);
}
