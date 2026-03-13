using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntQuestGroupTable : TableBase<EntityMBigHuntQuestGroup>
{
    private readonly Func<EntityMBigHuntQuestGroup, (int, int)> primaryIndexSelector;

    public EntityMBigHuntQuestGroupTable(EntityMBigHuntQuestGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BigHuntQuestGroupId, element.SortOrder);
    }
}
