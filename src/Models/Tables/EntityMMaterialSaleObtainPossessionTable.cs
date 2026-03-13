using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMaterialSaleObtainPossessionTable : TableBase<EntityMMaterialSaleObtainPossession>
{
    private readonly Func<EntityMMaterialSaleObtainPossession, (int, int)> primaryIndexSelector;

    public EntityMMaterialSaleObtainPossessionTable(EntityMMaterialSaleObtainPossession[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.MaterialSaleObtainPossessionId, element.SortOrder);
    }
}
