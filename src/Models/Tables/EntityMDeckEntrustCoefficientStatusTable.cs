using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMDeckEntrustCoefficientStatusTable : TableBase<EntityMDeckEntrustCoefficientStatus>
{
    private readonly Func<EntityMDeckEntrustCoefficientStatus, (int, int)> primaryIndexSelector;

    public EntityMDeckEntrustCoefficientStatusTable(EntityMDeckEntrustCoefficientStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EntrustDeckStatusType, element.DeckStatusType);
    }
}
