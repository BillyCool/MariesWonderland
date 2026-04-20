using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickGroupEventLog))]
public class EntityMGimmickGroupEventLog
{
    [Key(0)] public int GimmickGroupId { get; set; }

    [Key(1)] public int EventLogTextId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
