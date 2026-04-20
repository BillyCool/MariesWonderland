using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickOrnament))]
public class EntityMGimmickOrnament
{
    [Key(0)] public int GimmickOrnamentGroupId { get; set; }

    [Key(1)] public int GimmickOrnamentIndex { get; set; }

    [Key(2)] public int GimmickOrnamentViewId { get; set; }

    [Key(3)] public int Count { get; set; }

    [Key(4)] public int ChapterId { get; set; }

    [Key(5)] public float PositionX { get; set; }

    [Key(6)] public float PositionY { get; set; }

    [Key(7)] public float PositionZ { get; set; }

    [Key(8)] public float Rotation { get; set; }

    [Key(9)] public float Scale { get; set; }

    [Key(10)] public int SortOrder { get; set; }

    [Key(11)] public int AssetBackgroundId { get; set; }

    [Key(12)] public int IconDifficultyValue { get; set; }

    [Key(13)] public float RotationX { get; set; }

    [Key(14)] public float RotationZ { get; set; }
}
