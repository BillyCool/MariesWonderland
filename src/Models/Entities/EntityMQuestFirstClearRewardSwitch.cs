using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestFirstClearRewardSwitch))]
[PrimaryKey(nameof(QuestId))]
public class EntityMQuestFirstClearRewardSwitch
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int QuestFirstClearRewardGroupId { get; set; }

    [Key(2)] public int SwitchConditionClearQuestId { get; set; }
}
