using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMTitleStillTable : TableBase<EntityMTitleStill>
{
    private readonly Func<EntityMTitleStill, int> primaryIndexSelector;
    private readonly Func<EntityMTitleStill, int> secondaryIndexSelector;

    public EntityMTitleStillTable(EntityMTitleStill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.TitleStillId;
        secondaryIndexSelector = element => element.TitleStillGroupId;
    }

    public EntityMTitleStill FindByTitleStillId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public RangeView<EntityMTitleStill> FindByTitleStillGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
