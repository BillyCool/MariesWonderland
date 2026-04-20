using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionClearConditionValueViewTable : TableBase<EntityMMissionClearConditionValueView>
{
    private readonly Func<EntityMMissionClearConditionValueView, MissionClearConditionType> primaryIndexSelector;

    public EntityMMissionClearConditionValueViewTable(EntityMMissionClearConditionValueView[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionClearConditionType;
    }

    public bool TryFindByMissionClearConditionType(MissionClearConditionType key, out EntityMMissionClearConditionValueView result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<MissionClearConditionType>.Default, key, out result);
}
