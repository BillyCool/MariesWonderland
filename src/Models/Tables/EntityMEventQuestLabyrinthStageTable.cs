using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLabyrinthStageTable : TableBase<EntityMEventQuestLabyrinthStage>
{
    private readonly Func<EntityMEventQuestLabyrinthStage, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMEventQuestLabyrinthStage, int> secondaryIndexSelector;

    public EntityMEventQuestLabyrinthStageTable(EntityMEventQuestLabyrinthStage[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestChapterId, element.StageOrder);
        secondaryIndexSelector = element => element.EventQuestChapterId;
    }

    public EntityMEventQuestLabyrinthStage FindByEventQuestChapterIdAndStageOrder(ValueTuple<int, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public RangeView<EntityMEventQuestLabyrinthStage> FindByEventQuestChapterId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
