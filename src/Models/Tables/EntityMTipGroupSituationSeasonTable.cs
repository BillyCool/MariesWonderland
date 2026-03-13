using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMTipGroupSituationSeasonTable : TableBase<EntityMTipGroupSituationSeason>
{
    private readonly Func<EntityMTipGroupSituationSeason, (TipSituationType, int, int)> primaryIndexSelector;

    public EntityMTipGroupSituationSeasonTable(EntityMTipGroupSituationSeason[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.TipSituationType, element.TipSituationSeasonId, element.TipGroupId);
    }
}
