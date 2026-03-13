using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMDeckEntrustCoefficientPartsSeriesBonusCountTable : TableBase<EntityMDeckEntrustCoefficientPartsSeriesBonusCount>
{
    private readonly Func<EntityMDeckEntrustCoefficientPartsSeriesBonusCount, int> primaryIndexSelector;

    public EntityMDeckEntrustCoefficientPartsSeriesBonusCountTable(EntityMDeckEntrustCoefficientPartsSeriesBonusCount[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PartsSeriesBonusCount;
    }
}
