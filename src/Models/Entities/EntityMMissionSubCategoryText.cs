using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionSubCategoryText))]
public class EntityMMissionSubCategoryText
{
    [Key(0)] public int MissionSubCategoryId { get; set; }

    [Key(1)] public int TextId { get; set; }
}
