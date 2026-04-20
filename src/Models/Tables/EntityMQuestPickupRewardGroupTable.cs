using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestPickupRewardGroupTable : TableBase<EntityMQuestPickupRewardGroup>
{
    private readonly Func<EntityMQuestPickupRewardGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestPickupRewardGroupTable(EntityMQuestPickupRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestPickupRewardGroupId, element.SortOrder);
    }
}
