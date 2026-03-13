using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEnhanceCampaignTable : TableBase<EntityMEnhanceCampaign>
{
    private readonly Func<EntityMEnhanceCampaign, int> primaryIndexSelector;

    public EntityMEnhanceCampaignTable(EntityMEnhanceCampaign[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EnhanceCampaignId;
    }
}
