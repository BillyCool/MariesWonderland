using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickAdditionalAsset))]
public class EntityMGimmickAdditionalAsset
{
    [Key(0)] public int GimmickId { get; set; }

    [Key(1)] public string GimmickTexturePath { get; set; }
}
