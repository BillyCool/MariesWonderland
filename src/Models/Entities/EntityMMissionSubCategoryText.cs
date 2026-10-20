using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionSubCategoryText))]
[PrimaryKey(nameof(MissionSubCategoryId))]
public class EntityMMissionSubCategoryText
{
    [Key(0)] public int MissionSubCategoryId { get; set; }

    [Key(1)] public int TextId { get; set; }
}
