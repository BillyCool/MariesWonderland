using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMImportantItemEffectUnlockFunctionTable : TableBase<EntityMImportantItemEffectUnlockFunction>
{
    private readonly Func<EntityMImportantItemEffectUnlockFunction, int> primaryIndexSelector;

    public EntityMImportantItemEffectUnlockFunctionTable(EntityMImportantItemEffectUnlockFunction[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ImportantItemEffectUnlockFunctionId;
    }
}
