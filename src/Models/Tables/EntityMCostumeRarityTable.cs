using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeRarityTable : TableBase<EntityMCostumeRarity>
{
    private readonly Func<EntityMCostumeRarity, RarityType> primaryIndexSelector;

    public EntityMCostumeRarityTable(EntityMCostumeRarity[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.RarityType;
    }

    public EntityMCostumeRarity FindByRarityType(RarityType key) => FindUniqueCore(data, primaryIndexSelector, Comparer<RarityType>.Default, key);
}
