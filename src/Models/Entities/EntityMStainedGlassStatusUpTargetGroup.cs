using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMStainedGlassStatusUpTargetGroup
{
    public int StainedGlassStatusUpTargetGroupId { get; set; }

    public int GroupIndex { get; set; }

    public StainedGlassStatusUpTargetType StatusUpTargetType { get; set; }

    public int TargetValue { get; set; }
}
