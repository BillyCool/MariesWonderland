using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmick))]
public class EntityMGimmick
{
    [Key(0)] public int GimmickId { get; set; }

    [Key(1)] public GimmickType GimmickType { get; set; }

    [Key(2)] public int GimmickOrnamentGroupId { get; set; }

    [Key(3)] public int ClearEvaluateConditionId { get; set; }

    [Key(4)] public int ReleaseEvaluateConditionId { get; set; }
}
