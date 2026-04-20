using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserEventQuestLabyrinthStageTable : TableBase<EntityIUserEventQuestLabyrinthStage>
{
    private readonly Func<EntityIUserEventQuestLabyrinthStage, (long, int, int)> primaryIndexSelector;

    public EntityIUserEventQuestLabyrinthStageTable(EntityIUserEventQuestLabyrinthStage[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.EventQuestChapterId, element.StageOrder);
    }

    public EntityIUserEventQuestLabyrinthStage FindByUserIdAndEventQuestChapterIdAndStageOrder(ValueTuple<long, int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, int)>.Default, key);

    public bool TryFindByUserIdAndEventQuestChapterIdAndStageOrder(ValueTuple<long, int, int> key, out EntityIUserEventQuestLabyrinthStage result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, int)>.Default, key, out result);
}
