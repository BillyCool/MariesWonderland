using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBeginnerCampaignTable : TableBase<EntityMBeginnerCampaign>
{
    private readonly Func<EntityMBeginnerCampaign, int> primaryIndexSelector;

    public EntityMBeginnerCampaignTable(EntityMBeginnerCampaign[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BeginnerCampaignId;
    }

    public EntityMBeginnerCampaign FindByBeginnerCampaignId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
