using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestDisplayItemGroupTable : TableBase<EntityMEventQuestDisplayItemGroup>
{
    private readonly Func<EntityMEventQuestDisplayItemGroup, (int, int)> primaryIndexSelector;

    public EntityMEventQuestDisplayItemGroupTable(EntityMEventQuestDisplayItemGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestDisplayItemGroupId, element.SortOrder);
    }
}
