using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeDisplayCoordinateAdjustment
{
    public int CostumeId { get; set; }

    public DisplayCoordinateAdjustmentFunctionType DisplayCoordinateAdjustmentFunctionType { get; set; }

    public int HorizontalCoordinateCountPermil { get; set; }

    public int VerticalCoordinateCountPermil { get; set; }
}
