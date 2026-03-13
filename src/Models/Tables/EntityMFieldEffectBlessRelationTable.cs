using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMFieldEffectBlessRelationTable : TableBase<EntityMFieldEffectBlessRelation>
{
    private readonly Func<EntityMFieldEffectBlessRelation, (int, int)> primaryIndexSelector;

    public EntityMFieldEffectBlessRelationTable(EntityMFieldEffectBlessRelation[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.FieldEffectGroupId, element.FieldEffectBlessRelationIndex);
    }
}
