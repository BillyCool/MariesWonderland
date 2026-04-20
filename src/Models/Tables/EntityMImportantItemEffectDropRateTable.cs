using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMImportantItemEffectDropRateTable : TableBase<EntityMImportantItemEffectDropRate>
{
    private readonly Func<EntityMImportantItemEffectDropRate, int> primaryIndexSelector;

    public EntityMImportantItemEffectDropRateTable(EntityMImportantItemEffectDropRate[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ImportantItemEffectDropRateId;
    }
}
