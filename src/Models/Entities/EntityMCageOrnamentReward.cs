using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCageOrnamentReward))]
[PrimaryKey(nameof(CageOrnamentRewardId), nameof(PossessionType), nameof(PossessionId))]
public class EntityMCageOrnamentReward
{
    [Key(0)] public int CageOrnamentRewardId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int Count { get; set; }
}
