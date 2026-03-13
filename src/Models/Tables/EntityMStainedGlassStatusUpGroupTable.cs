using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMStainedGlassStatusUpGroupTable : TableBase<EntityMStainedGlassStatusUpGroup>
{
    private readonly Func<EntityMStainedGlassStatusUpGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMStainedGlassStatusUpGroup, int> secondaryIndexSelector;

    public EntityMStainedGlassStatusUpGroupTable(EntityMStainedGlassStatusUpGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.StainedGlassStatusUpGroupId, element.GroupIndex);
        secondaryIndexSelector = element => element.StainedGlassStatusUpGroupId;
    }

    public RangeView<EntityMStainedGlassStatusUpGroup> FindByStainedGlassStatusUpGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
