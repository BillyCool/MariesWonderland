using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionLinkTable : TableBase<EntityMMissionLink>
{
    private readonly Func<EntityMMissionLink, int> primaryIndexSelector;

    public EntityMMissionLinkTable(EntityMMissionLink[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionLinkId;
    }

    public EntityMMissionLink FindByMissionLinkId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
