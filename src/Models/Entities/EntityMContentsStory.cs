using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMContentsStory))]
public class EntityMContentsStory
{
    [Key(0)] public int ContentsStoryId { get; set; }

    [Key(1)] public QuestSceneType QuestSceneType { get; set; }

    [Key(2)] public int AssetBackgroundId { get; set; }

    [Key(3)] public int EventMapNumberUpper { get; set; }

    [Key(4)] public int EventMapNumberLower { get; set; }

    [Key(5)] public bool IsForcedPlay { get; set; }

    [Key(6)] public UnlockConditionType ContentsStoryUnlockConditionType { get; set; }

    [Key(7)] public int ConditionValue { get; set; }

    [Key(8)] public int UnlockEvaluateConditionId { get; set; }
}
