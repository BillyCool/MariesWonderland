using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMaterial
{
    public int MaterialId { get; set; }

    public MaterialType MaterialType { get; set; }

    public RarityType RarityType { get; set; }

    public WeaponType WeaponType { get; set; }

    public AttributeType AttributeType { get; set; }

    public int EffectValue { get; set; }

    public int SellPrice { get; set; }

    public string AssetName { get; set; }

    public int AssetCategoryId { get; set; }

    public int AssetVariationId { get; set; }

    public int MaterialSaleObtainPossessionId { get; set; }
}
