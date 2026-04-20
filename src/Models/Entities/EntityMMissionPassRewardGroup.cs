using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionPassRewardGroup))]
public class EntityMMissionPassRewardGroup
{
    [Key(0)] public int MissionPassRewardGroupId { get; set; }

    [Key(1)] public int Level { get; set; }

    [Key(2)] public bool IsPremium { get; set; }

    [Key(3)] public int SortOrder { get; set; }

    [Key(4)] public PossessionType PossessionType { get; set; }

    [Key(5)] public int PossessionId { get; set; }

    [Key(6)] public int Count { get; set; }
}
