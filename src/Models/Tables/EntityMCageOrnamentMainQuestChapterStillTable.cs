using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCageOrnamentMainQuestChapterStillTable : TableBase<EntityMCageOrnamentMainQuestChapterStill>
{
    private readonly Func<EntityMCageOrnamentMainQuestChapterStill, int> primaryIndexSelector;

    public EntityMCageOrnamentMainQuestChapterStillTable(EntityMCageOrnamentMainQuestChapterStill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MainQuestChapterId;
    }

    public EntityMCageOrnamentMainQuestChapterStill FindByMainQuestChapterId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
