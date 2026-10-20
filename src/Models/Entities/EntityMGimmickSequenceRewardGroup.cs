using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickSequenceRewardGroup))]
[PrimaryKey(nameof(GimmickSequenceRewardGroupId), nameof(GroupIndex))]
public class EntityMGimmickSequenceRewardGroup
{
    [Key(0)] public int GimmickSequenceRewardGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public PossessionType PossessionType { get; set; }

    [Key(3)] public int PossessionId { get; set; }

    [Key(4)] public int Count { get; set; }
}
