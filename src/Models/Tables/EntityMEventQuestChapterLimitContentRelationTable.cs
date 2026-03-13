using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestChapterLimitContentRelationTable : TableBase<EntityMEventQuestChapterLimitContentRelation>
{
    private readonly Func<EntityMEventQuestChapterLimitContentRelation, int> primaryIndexSelector;
    private readonly Func<EntityMEventQuestChapterLimitContentRelation, int> secondaryIndexSelector;

    public EntityMEventQuestChapterLimitContentRelationTable(EntityMEventQuestChapterLimitContentRelation[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EventQuestChapterId;
        secondaryIndexSelector = element => element.EventQuestLimitContentId;
    }

    public EntityMEventQuestChapterLimitContentRelation FindByEventQuestChapterId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByEventQuestChapterId(int key, out EntityMEventQuestChapterLimitContentRelation result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);

    public RangeView<EntityMEventQuestChapterLimitContentRelation> FindByEventQuestLimitContentId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
