using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponRarityTable : TableBase<EntityMWeaponRarity>
{
    private readonly Func<EntityMWeaponRarity, RarityType> primaryIndexSelector;

    public EntityMWeaponRarityTable(EntityMWeaponRarity[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.RarityType;
    }

    public EntityMWeaponRarity FindByRarityType(RarityType key) => FindUniqueCore(data, primaryIndexSelector, Comparer<RarityType>.Default, key);
}
