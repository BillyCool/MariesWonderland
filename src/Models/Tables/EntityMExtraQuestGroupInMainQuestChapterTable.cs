using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMExtraQuestGroupInMainQuestChapterTable : TableBase<EntityMExtraQuestGroupInMainQuestChapter>
{
    private readonly Func<EntityMExtraQuestGroupInMainQuestChapter, (int, int)> primaryIndexSelector;

    public EntityMExtraQuestGroupInMainQuestChapterTable(EntityMExtraQuestGroupInMainQuestChapter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.MainQuestChapterId, element.ExtraQuestIndex);
    }

    public bool TryFindByMainQuestChapterIdAndExtraQuestIndex(ValueTuple<int, int> key, out EntityMExtraQuestGroupInMainQuestChapter result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);
}
