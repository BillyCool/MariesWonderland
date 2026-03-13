using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMContentsStory
{
    public int ContentsStoryId { get; set; }

    public QuestSceneType QuestSceneType { get; set; }

    public int AssetBackgroundId { get; set; }

    public int EventMapNumberUpper { get; set; }

    public int EventMapNumberLower { get; set; }

    public bool IsForcedPlay { get; set; }

    public UnlockConditionType ContentsStoryUnlockConditionType { get; set; }

    public int ConditionValue { get; set; }

    public int UnlockEvaluateConditionId { get; set; }
}
