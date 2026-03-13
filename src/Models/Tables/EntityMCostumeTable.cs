using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeTable : TableBase<EntityMCostume>
{
    private readonly Func<EntityMCostume, int> primaryIndexSelector;
    private readonly Func<EntityMCostume, int> secondaryIndexSelector;

    public EntityMCostumeTable(EntityMCostume[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeId;
        secondaryIndexSelector = element => element.CharacterId;
    }

    public EntityMCostume FindByCostumeId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByCostumeId(int key, out EntityMCostume result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);

    public RangeView<EntityMCostume> FindByCharacterId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
