using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMStainedGlassTable : TableBase<EntityMStainedGlass>
{
    private readonly Func<EntityMStainedGlass, int> primaryIndexSelector;

    public EntityMStainedGlassTable(EntityMStainedGlass[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.StainedGlassId;
    }

    public EntityMStainedGlass FindByStainedGlassId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
