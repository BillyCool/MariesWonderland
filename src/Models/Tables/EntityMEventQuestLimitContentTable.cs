using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLimitContentTable : TableBase<EntityMEventQuestLimitContent>
{
    private readonly Func<EntityMEventQuestLimitContent, int> primaryIndexSelector;

    public EntityMEventQuestLimitContentTable(EntityMEventQuestLimitContent[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EventQuestLimitContentId;
    }

    public EntityMEventQuestLimitContent FindByEventQuestLimitContentId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByEventQuestLimitContentId(int key, out EntityMEventQuestLimitContent result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
