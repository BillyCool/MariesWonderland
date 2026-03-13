using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserBeginnerCampaignTable : TableBase<EntityIUserBeginnerCampaign>
{
    private readonly Func<EntityIUserBeginnerCampaign, long> primaryIndexSelector;

    public EntityIUserBeginnerCampaignTable(EntityIUserBeginnerCampaign[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserBeginnerCampaign FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
