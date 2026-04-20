using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMHelpPageGroup))]
public class EntityMHelpPageGroup
{
    [Key(0)] public int HelpPageGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int HelpPageId { get; set; }
}
