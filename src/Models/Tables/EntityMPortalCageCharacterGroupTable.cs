using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPortalCageCharacterGroupTable : TableBase<EntityMPortalCageCharacterGroup>
{
    private readonly Func<EntityMPortalCageCharacterGroup, int> primaryIndexSelector;

    public EntityMPortalCageCharacterGroupTable(EntityMPortalCageCharacterGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PortalCageCharacterGroupId;
    }

    public bool TryFindByPortalCageCharacterGroupId(int key, out EntityMPortalCageCharacterGroup result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
