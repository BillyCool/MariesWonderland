using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMComebackCampaignTable : TableBase<EntityMComebackCampaign>
{
    private readonly Func<EntityMComebackCampaign, int> primaryIndexSelector;

    public EntityMComebackCampaignTable(EntityMComebackCampaign[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ComebackCampaignId;
    }

    public EntityMComebackCampaign FindByComebackCampaignId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
