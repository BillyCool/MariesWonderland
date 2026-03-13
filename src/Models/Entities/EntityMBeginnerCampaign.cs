using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBeginnerCampaign
{
    public int BeginnerCampaignId { get; set; }

    public long BeginnerJudgeStartDatetime { get; set; }

    public long BeginnerJudgeEndDatetime { get; set; }

    public int GrantCampaignTermDayCount { get; set; }

    public int CampaignUnlockQuestId { get; set; }
}
