using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMaterialTable : TableBase<EntityIUserMaterial>
{
    private readonly Func<EntityIUserMaterial, (long, int)> primaryIndexSelector;

    public EntityIUserMaterialTable(EntityIUserMaterial[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.MaterialId);
    }

    public EntityIUserMaterial FindByUserIdAndMaterialId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
