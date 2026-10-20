using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMHelp))]
[PrimaryKey(nameof(HelpType))]
public class EntityMHelp
{
    [Key(0)] public HelpType HelpType { get; set; }

    [Key(1)] public int HelpItemId { get; set; }

    [Key(2)] public int HelpPageGroupId { get; set; }
}
