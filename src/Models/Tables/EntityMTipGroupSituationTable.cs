using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMTipGroupSituationTable : TableBase<EntityMTipGroupSituation>
{
    private readonly Func<EntityMTipGroupSituation, (TipSituationType, int)> primaryIndexSelector;

    public EntityMTipGroupSituationTable(EntityMTipGroupSituation[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.TipSituationType, element.TipGroupId);
    }
}
