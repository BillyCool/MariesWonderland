using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLabyrinthQuestEffectDescriptionFreeTable : TableBase<EntityMEventQuestLabyrinthQuestEffectDescriptionFree>
{
    private readonly Func<EntityMEventQuestLabyrinthQuestEffectDescriptionFree, int> primaryIndexSelector;

    public EntityMEventQuestLabyrinthQuestEffectDescriptionFreeTable(EntityMEventQuestLabyrinthQuestEffectDescriptionFree[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EventQuestLabyrinthQuestEffectDescriptionId;
    }

    public EntityMEventQuestLabyrinthQuestEffectDescriptionFree FindByEventQuestLabyrinthQuestEffectDescriptionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
