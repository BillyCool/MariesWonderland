using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestDisplayEnemyThumbnailReplace
{
    public int QuestId { get; set; }

    public int Priority { get; set; }

    public EnemyThumbnailReplaceConditionType ReplaceConditionType { get; set; }

    public EnemyThumbnailReplaceMethodType ReplaceMethodType { get; set; }

    public int ReplaceValue { get; set; }
}
