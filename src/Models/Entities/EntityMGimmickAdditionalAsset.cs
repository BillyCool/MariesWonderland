using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickAdditionalAsset))]
[PrimaryKey(nameof(GimmickId))]
public class EntityMGimmickAdditionalAsset
{
    [Key(0)] public int GimmickId { get; set; }

    [Key(1)] public string GimmickTexturePath { get; set; }
}
