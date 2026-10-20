using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeEnhanced))]
[PrimaryKey(nameof(CostumeEnhancedId))]
public class EntityMCostumeEnhanced
{
    [Key(0)] public int CostumeEnhancedId { get; set; }

    [Key(1)] public int CostumeId { get; set; }

    [Key(2)] public int LimitBreakCount { get; set; }

    [Key(3)] public int Level { get; set; }

    [Key(4)] public int ActiveSkillLevel { get; set; }

    [Key(5)] public int AwakenCount { get; set; }
}
