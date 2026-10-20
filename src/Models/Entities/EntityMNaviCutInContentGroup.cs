using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNaviCutInContentGroup))]
[PrimaryKey(nameof(NaviCutInContentGroupId), nameof(ContentIndex))]
public class EntityMNaviCutInContentGroup
{
    [Key(0)] public int NaviCutInContentGroupId { get; set; }

    [Key(1)] public int ContentIndex { get; set; }

    [Key(2)] public int NaviCutInTextId { get; set; }
}
