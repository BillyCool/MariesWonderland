using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMHelpPageGroup))]
[PrimaryKey(nameof(HelpPageGroupId), nameof(SortOrder))]
public class EntityMHelpPageGroup
{
    [Key(0)] public int HelpPageGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int HelpPageId { get; set; }
}
