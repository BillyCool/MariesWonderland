using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeEmblemTable : TableBase<EntityMCostumeEmblem>
{
    private readonly Func<EntityMCostumeEmblem, int> primaryIndexSelector;

    public EntityMCostumeEmblemTable(EntityMCostumeEmblem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeEmblemAssetId;
    }

    public EntityMCostumeEmblem FindByCostumeEmblemAssetId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
