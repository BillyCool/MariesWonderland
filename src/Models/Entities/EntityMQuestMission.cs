using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestMission))]
public class EntityMQuestMission
{
    [Key(0)] public int QuestMissionId { get; set; }

    [Key(1)] public QuestMissionConditionType QuestMissionConditionType { get; set; }

    [Key(2)] public int ConditionValue { get; set; }

    [Key(3)] public int QuestMissionRewardId { get; set; }

    [Key(4)] public int QuestMissionConditionValueGroupId { get; set; }
}
