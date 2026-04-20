using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickGroup))]
public class EntityMGimmickGroup
{
    [Key(0)] public int GimmickGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public int GimmickId { get; set; }
}
