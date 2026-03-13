using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMGimmickOrnament
{
    public int GimmickOrnamentGroupId { get; set; }

    public int GimmickOrnamentIndex { get; set; }

    public int GimmickOrnamentViewId { get; set; }

    public int Count { get; set; }

    public int ChapterId { get; set; }

    public float PositionX { get; set; }

    public float PositionY { get; set; }

    public float PositionZ { get; set; }

    public float Rotation { get; set; }

    public float Scale { get; set; }

    public int SortOrder { get; set; }

    public int AssetBackgroundId { get; set; }

    public int IconDifficultyValue { get; set; }

    public float RotationX { get; set; }

    public float RotationZ { get; set; }
}
