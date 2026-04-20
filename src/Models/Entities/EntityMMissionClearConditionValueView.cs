using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionClearConditionValueView))]
public class EntityMMissionClearConditionValueView
{
    [Key(0)] public MissionClearConditionType MissionClearConditionType { get; set; }

    [Key(1)] public int ViewClearConditionValue { get; set; }
}
