using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestScheduleCorrespondence))]
public class EntityMQuestScheduleCorrespondence
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int QuestScheduleId { get; set; }
}
