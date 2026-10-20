using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAnimationStep))]
[PrimaryKey(nameof(CostumeId), nameof(Step), nameof(ActorAnimationId))]
public class EntityMCostumeAnimationStep
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int Step { get; set; }

    [Key(2)] public int ActorAnimationId { get; set; }
}
