using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeDisplayCoordinateAdjustment))]
public class EntityMCostumeDisplayCoordinateAdjustment
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public DisplayCoordinateAdjustmentFunctionType DisplayCoordinateAdjustmentFunctionType { get; set; }

    [Key(2)] public int HorizontalCoordinateCountPermil { get; set; }

    [Key(3)] public int VerticalCoordinateCountPermil { get; set; }
}
