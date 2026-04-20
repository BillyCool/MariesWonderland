using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMStainedGlass))]
public class EntityMStainedGlass
{
    [Key(0)] public int StainedGlassId { get; set; }

    [Key(1)] public StainedGlassCategoryType StainedGlassCategoryType { get; set; }

    [Key(2)] public int ImageAssetId { get; set; }

    [Key(3)] public int TitleTextId { get; set; }

    [Key(4)] public int EffectDescriptionTextId { get; set; }

    [Key(5)] public int FlavorTextId { get; set; }

    [Key(6)] public int SortOrder { get; set; }

    [Key(7)] public long EmptyDisplayStartDatetime { get; set; }

    [Key(8)] public long LockDisplayStartDatetime { get; set; }

    [Key(9)] public int StainedGlassStatusUpTargetGroupId { get; set; }

    [Key(10)] public int StainedGlassStatusUpGroupId { get; set; }
}
