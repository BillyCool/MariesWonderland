using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestChapterDifficultyLimitContentUnlockTable : TableBase<EntityMEventQuestChapterDifficultyLimitContentUnlock>
{
    private readonly Func<EntityMEventQuestChapterDifficultyLimitContentUnlock, (int, DifficultyType)> primaryIndexSelector;
    private readonly Func<EntityMEventQuestChapterDifficultyLimitContentUnlock, int> secondaryIndexSelector;

    public EntityMEventQuestChapterDifficultyLimitContentUnlockTable(EntityMEventQuestChapterDifficultyLimitContentUnlock[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestChapterId, element.DifficultyType);
        secondaryIndexSelector = element => element.EventQuestChapterId;
    }

    public bool TryFindByEventQuestChapterIdAndDifficultyType(ValueTuple<int, DifficultyType> key, out EntityMEventQuestChapterDifficultyLimitContentUnlock result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, DifficultyType)>.Default, key, out result);
}
