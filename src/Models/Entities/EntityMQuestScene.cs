using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestScene))]
public class EntityMQuestScene
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public int QuestId { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public QuestSceneType QuestSceneType { get; set; }

    [Key(4)] public int AssetBackgroundId { get; set; }

    [Key(5)] public int EventMapNumberUpper { get; set; }

    [Key(6)] public int EventMapNumberLower { get; set; }

    [Key(7)] public bool IsMainFlowQuestTarget { get; set; }

    [Key(8)] public bool IsBattleOnlyTarget { get; set; }

    [Key(9)] public QuestResultType QuestResultType { get; set; }

    [Key(10)] public bool IsStorySkipTarget { get; set; }
}
