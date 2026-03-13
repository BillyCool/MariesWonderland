using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMStainedGlass
{
    public int StainedGlassId { get; set; }

    public StainedGlassCategoryType StainedGlassCategoryType { get; set; }

    public int ImageAssetId { get; set; }

    public int TitleTextId { get; set; }

    public int EffectDescriptionTextId { get; set; }

    public int FlavorTextId { get; set; }

    public int SortOrder { get; set; }

    public long EmptyDisplayStartDatetime { get; set; }

    public long LockDisplayStartDatetime { get; set; }

    public int StainedGlassStatusUpTargetGroupId { get; set; }

    public int StainedGlassStatusUpGroupId { get; set; }
}
