using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMHelp))]
public class EntityMHelp
{
    [Key(0)] public HelpType HelpType { get; set; }

    [Key(1)] public int HelpItemId { get; set; }

    [Key(2)] public int HelpPageGroupId { get; set; }
}
