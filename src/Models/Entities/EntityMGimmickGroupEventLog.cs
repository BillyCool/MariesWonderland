using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickGroupEventLog))]
[PrimaryKey(nameof(GimmickGroupId))]
public class EntityMGimmickGroupEventLog
{
    [Key(0)] public int GimmickGroupId { get; set; }

    [Key(1)] public int EventLogTextId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
