using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLabyrinthSeasonTable : TableBase<EntityMEventQuestLabyrinthSeason>
{
    private readonly Func<EntityMEventQuestLabyrinthSeason, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMEventQuestLabyrinthSeason, int> secondaryIndexSelector;

    public EntityMEventQuestLabyrinthSeasonTable(EntityMEventQuestLabyrinthSeason[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestChapterId, element.SeasonNumber);
        secondaryIndexSelector = element => element.EventQuestChapterId;
    }

    public EntityMEventQuestLabyrinthSeason FindByEventQuestChapterIdAndSeasonNumber(ValueTuple<int, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public RangeView<EntityMEventQuestLabyrinthSeason> FindByEventQuestChapterId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
