using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCageOrnamentStillReleaseConditionTable : TableBase<EntityMCageOrnamentStillReleaseCondition>
{
    private readonly Func<EntityMCageOrnamentStillReleaseCondition, (int, int)> primaryIndexSelector;

    public EntityMCageOrnamentStillReleaseConditionTable(EntityMCageOrnamentStillReleaseCondition[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CageOrnamentStillReleaseConditionId, element.CageOrnamentId);
    }
}
