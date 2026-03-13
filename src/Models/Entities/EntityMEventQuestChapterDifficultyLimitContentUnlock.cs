using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestChapterDifficultyLimitContentUnlock
{
    public int EventQuestChapterId { get; set; }

    public DifficultyType DifficultyType { get; set; }

    public int UnlockEvaluateConditionId { get; set; }
}
