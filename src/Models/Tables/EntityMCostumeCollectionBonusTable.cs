using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeCollectionBonusTable : TableBase<EntityMCostumeCollectionBonus>
{
    private readonly Func<EntityMCostumeCollectionBonus, int> primaryIndexSelector;

    public EntityMCostumeCollectionBonusTable(EntityMCostumeCollectionBonus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CollectionBonusId;
    }

    public RangeView<EntityMCostumeCollectionBonus> FindRangeByCollectionBonusId(int min, int max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<int>.Default, min, max, ascendant);
}
