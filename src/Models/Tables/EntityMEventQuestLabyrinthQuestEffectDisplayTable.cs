using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLabyrinthQuestEffectDisplayTable : TableBase<EntityMEventQuestLabyrinthQuestEffectDisplay>
{
    private readonly Func<EntityMEventQuestLabyrinthQuestEffectDisplay, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMEventQuestLabyrinthQuestEffectDisplay, int> secondaryIndexSelector;

    public EntityMEventQuestLabyrinthQuestEffectDisplayTable(EntityMEventQuestLabyrinthQuestEffectDisplay[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestId, element.SortOrder);
        secondaryIndexSelector = element => element.QuestId;
    }

    public RangeView<EntityMEventQuestLabyrinthQuestEffectDisplay> FindByQuestId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
