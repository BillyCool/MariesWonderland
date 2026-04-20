using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMDokanContentGroup))]
public class EntityMDokanContentGroup
{
    [Key(0)] public int DokanContentGroupId { get; set; }

    [Key(1)] public int ContentIndex { get; set; }

    [Key(2)] public int ImageId { get; set; }

    [Key(3)] public int MovieId { get; set; }

    [Key(4)] public int DokanTextId { get; set; }
}
