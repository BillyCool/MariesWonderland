using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickGroup))]
[PrimaryKey(nameof(GimmickGroupId), nameof(GroupIndex))]
public class EntityMGimmickGroup
{
    [Key(0)] public int GimmickGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public int GimmickId { get; set; }
}
