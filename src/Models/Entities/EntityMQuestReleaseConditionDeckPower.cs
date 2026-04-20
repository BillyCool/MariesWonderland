using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestReleaseConditionDeckPower))]
public class EntityMQuestReleaseConditionDeckPower
{
    [Key(0)] public int QuestReleaseConditionId { get; set; }

    [Key(1)] public int MaxDeckPower { get; set; }
}
