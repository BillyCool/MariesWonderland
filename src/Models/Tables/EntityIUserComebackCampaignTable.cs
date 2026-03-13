using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserComebackCampaignTable : TableBase<EntityIUserComebackCampaign>
{
    private readonly Func<EntityIUserComebackCampaign, long> primaryIndexSelector;

    public EntityIUserComebackCampaignTable(EntityIUserComebackCampaign[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserComebackCampaign FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
