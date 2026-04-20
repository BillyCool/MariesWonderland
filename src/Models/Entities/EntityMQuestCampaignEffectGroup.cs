using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestCampaignEffectGroup))]
public class EntityMQuestCampaignEffectGroup
{
    [Key(0)] public int QuestCampaignEffectGroupId { get; set; }

    [Key(1)] public QuestCampaignEffectType QuestCampaignEffectType { get; set; }

    [Key(2)] public int QuestCampaignEffectValue { get; set; }

    [Key(3)] public int QuestCampaignTargetItemGroupId { get; set; }
}
