using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBigHuntBossQuest
{
    public int BigHuntBossQuestId { get; set; }

    public int BigHuntBossId { get; set; }

    public int BigHuntQuestGroupId { get; set; }

    public int BigHuntBossQuestScoreCoefficientId { get; set; }

    public int BigHuntScoreRewardGroupScheduleId { get; set; }

    public int BigHuntLinkId { get; set; }

    public int DailyChallengeCount { get; set; }
}
