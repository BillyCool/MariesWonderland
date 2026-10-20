using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestCampaignTargetGroup))]
[PrimaryKey(nameof(QuestCampaignTargetGroupId), nameof(QuestCampaignTargetIndex))]
public class EntityMQuestCampaignTargetGroup
{
    [Key(0)] public int QuestCampaignTargetGroupId { get; set; }

    [Key(1)] public int QuestCampaignTargetIndex { get; set; }

    [Key(2)] public QuestCampaignTargetType QuestCampaignTargetType { get; set; }

    [Key(3)] public int QuestCampaignTargetValue { get; set; }
}
