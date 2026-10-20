using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionClearConditionValueView))]
[PrimaryKey(nameof(MissionClearConditionType))]
public class EntityMMissionClearConditionValueView
{
    [Key(0)] public MissionClearConditionType MissionClearConditionType { get; set; }

    [Key(1)] public int ViewClearConditionValue { get; set; }
}
