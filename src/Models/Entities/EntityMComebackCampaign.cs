using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMComebackCampaign))]
public class EntityMComebackCampaign
{
    [Key(0)] public int ComebackCampaignId { get; set; }

    [Key(1)] public long ComebackJudgeStartDatetime { get; set; }

    [Key(2)] public long ComebackJudgeEndDatetime { get; set; }

    [Key(3)] public int ComebackJudgeDayCount { get; set; }

    [Key(4)] public int GrantCampaignTermDayCount { get; set; }

    [Key(5)] public int CampaignUnlockQuestId { get; set; }

    [Key(6)] public int ComebackCampaignGradeGroupId { get; set; }
}
