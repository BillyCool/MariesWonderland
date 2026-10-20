using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTutorialConsumePossessionGroup))]
[PrimaryKey(nameof(TutorialType), nameof(PossessionType), nameof(PossessionId))]
public class EntityMTutorialConsumePossessionGroup
{
    [Key(0)] public TutorialType TutorialType { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int Count { get; set; }
}
