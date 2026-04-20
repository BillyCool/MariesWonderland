using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCageOrnamentTable : TableBase<EntityMCageOrnament>
{
    private readonly Func<EntityMCageOrnament, int> primaryIndexSelector;

    public EntityMCageOrnamentTable(EntityMCageOrnament[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CageOrnamentId;
    }

    public EntityMCageOrnament FindByCageOrnamentId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByCageOrnamentId(int key, out EntityMCageOrnament result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);

    public RangeView<EntityMCageOrnament> FindRangeByCageOrnamentId(int min, int max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<int>.Default, min, max, ascendant);
}
