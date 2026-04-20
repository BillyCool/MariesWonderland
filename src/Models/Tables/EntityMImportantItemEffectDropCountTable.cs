using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMImportantItemEffectDropCountTable : TableBase<EntityMImportantItemEffectDropCount>
{
    private readonly Func<EntityMImportantItemEffectDropCount, int> primaryIndexSelector;

    public EntityMImportantItemEffectDropCountTable(EntityMImportantItemEffectDropCount[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ImportantItemEffectDropCountId;
    }
}
