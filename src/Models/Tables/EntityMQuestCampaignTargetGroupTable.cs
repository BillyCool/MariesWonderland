using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestCampaignTargetGroupTable : TableBase<EntityMQuestCampaignTargetGroup>
{
    private readonly Func<EntityMQuestCampaignTargetGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestCampaignTargetGroupTable(EntityMQuestCampaignTargetGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestCampaignTargetGroupId, element.QuestCampaignTargetIndex);
    }

    public RangeView<EntityMQuestCampaignTargetGroup> FindRangeByQuestCampaignTargetGroupIdAndQuestCampaignTargetIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
