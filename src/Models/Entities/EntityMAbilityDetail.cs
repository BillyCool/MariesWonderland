using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMAbilityDetail
{
    public int AbilityDetailId { get; set; }

    public int NameAbilityTextId { get; set; }

    public int DescriptionAbilityTextId { get; set; }

    public int AbilityBehaviourGroupId { get; set; }

    public int AssetCategoryId { get; set; }

    public int AssetVariationId { get; set; }
}
