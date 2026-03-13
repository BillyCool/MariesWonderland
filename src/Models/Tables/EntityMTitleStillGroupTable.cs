using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMTitleStillGroupTable : TableBase<EntityMTitleStillGroup>
{
    private readonly Func<EntityMTitleStillGroup, int> primaryIndexSelector;

    public EntityMTitleStillGroupTable(EntityMTitleStillGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.TitleStillGroupId;
    }

    public EntityMTitleStillGroup FindByTitleStillGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
