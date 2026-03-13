using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionTermTable : TableBase<EntityMMissionTerm>
{
    private readonly Func<EntityMMissionTerm, int> primaryIndexSelector;

    public EntityMMissionTermTable(EntityMMissionTerm[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionTermId;
    }

    public EntityMMissionTerm FindByMissionTermId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
