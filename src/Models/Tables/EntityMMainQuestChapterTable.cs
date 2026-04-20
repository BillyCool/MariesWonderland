using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMainQuestChapterTable : TableBase<EntityMMainQuestChapter>
{
    private readonly Func<EntityMMainQuestChapter, int> primaryIndexSelector;

    public EntityMMainQuestChapterTable(EntityMMainQuestChapter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MainQuestChapterId;
    }

    public EntityMMainQuestChapter FindByMainQuestChapterId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByMainQuestChapterId(int key, out EntityMMainQuestChapter result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
