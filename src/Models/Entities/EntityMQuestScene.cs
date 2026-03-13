using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestScene
{
    public int QuestSceneId { get; set; }

    public int QuestId { get; set; }

    public int SortOrder { get; set; }

    public QuestSceneType QuestSceneType { get; set; }

    public int AssetBackgroundId { get; set; }

    public int EventMapNumberUpper { get; set; }

    public int EventMapNumberLower { get; set; }

    public bool IsMainFlowQuestTarget { get; set; }

    public bool IsBattleOnlyTarget { get; set; }

    public QuestResultType QuestResultType { get; set; }

    public bool IsStorySkipTarget { get; set; }
}
