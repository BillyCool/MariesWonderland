using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMSideStoryQuestLimitContentTable : TableBase<EntityMSideStoryQuestLimitContent>
{
    private readonly Func<EntityMSideStoryQuestLimitContent, int> primaryIndexSelector;
    private readonly Func<EntityMSideStoryQuestLimitContent, (int, DifficultyType)> secondaryIndexSelector;

    public EntityMSideStoryQuestLimitContentTable(EntityMSideStoryQuestLimitContent[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SideStoryQuestLimitContentId;
        secondaryIndexSelector = element => (element.EventQuestChapterId, element.DifficultyType);
    }

    public EntityMSideStoryQuestLimitContent FindBySideStoryQuestLimitContentId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public RangeView<EntityMSideStoryQuestLimitContent> FindByEventQuestChapterIdAndDifficultyType(ValueTuple<int, DifficultyType> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(int, DifficultyType)>.Default, key);
}
