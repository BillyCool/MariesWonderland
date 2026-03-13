using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestReleaseConditionBigHuntScore
{
    public int QuestReleaseConditionId { get; set; }

    public int BigHuntBossId { get; set; }

    public long NecessaryScore { get; set; }
}
