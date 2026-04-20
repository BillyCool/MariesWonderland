using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionTable : TableBase<EntityMCompanion>
{
    private readonly Func<EntityMCompanion, int> primaryIndexSelector;

    public EntityMCompanionTable(EntityMCompanion[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CompanionId;
    }

    public EntityMCompanion FindByCompanionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
