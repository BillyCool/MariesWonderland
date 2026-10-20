using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTipGroup))]
[PrimaryKey(nameof(TipGroupId))]
public class EntityMTipGroup
{
    [Key(0)] public int TipGroupId { get; set; }

    [Key(1)] public int NameTextId { get; set; }
}
