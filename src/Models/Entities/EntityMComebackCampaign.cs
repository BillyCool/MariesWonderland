using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMComebackCampaign
{
    public int ComebackCampaignId { get; set; }

    public long ComebackJudgeStartDatetime { get; set; }

    public long ComebackJudgeEndDatetime { get; set; }

    public int ComebackJudgeDayCount { get; set; }

    public int GrantCampaignTermDayCount { get; set; }

    public int CampaignUnlockQuestId { get; set; }

    public int ComebackCampaignGradeGroupId { get; set; }
}
