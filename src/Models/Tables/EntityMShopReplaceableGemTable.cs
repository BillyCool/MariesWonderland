using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMShopReplaceableGemTable : TableBase<EntityMShopReplaceableGem>
{
    private readonly Func<EntityMShopReplaceableGem, int> primaryIndexSelector;

    public EntityMShopReplaceableGemTable(EntityMShopReplaceableGem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.LineupUpdateCountLowerLimit;
    }
}
