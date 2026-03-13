using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpSeasonGroupingTable : TableBase<EntityMPvpSeasonGrouping>
{
    private readonly Func<EntityMPvpSeasonGrouping, (int, int)> primaryIndexSelector;

    public EntityMPvpSeasonGroupingTable(EntityMPvpSeasonGrouping[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PvpSeasonGroupingId, element.GroupId);
    }
}
