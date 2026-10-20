using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMConsumableItemTerm))]
[PrimaryKey(nameof(ConsumableItemTermId))]
public class EntityMConsumableItemTerm
{
    [Key(0)] public int ConsumableItemTermId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }
}
