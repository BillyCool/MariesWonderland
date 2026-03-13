using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestDailyGroupTargetChapterTable : TableBase<EntityMEventQuestDailyGroupTargetChapter>
{
    private readonly Func<EntityMEventQuestDailyGroupTargetChapter, (int, int)> primaryIndexSelector;

    public EntityMEventQuestDailyGroupTargetChapterTable(EntityMEventQuestDailyGroupTargetChapter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestDailyGroupTargetChapterId, element.SortOrder);
    }
}
