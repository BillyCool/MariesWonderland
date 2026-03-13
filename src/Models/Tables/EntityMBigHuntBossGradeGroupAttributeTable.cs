using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntBossGradeGroupAttributeTable : TableBase<EntityMBigHuntBossGradeGroupAttribute>
{
    private readonly Func<EntityMBigHuntBossGradeGroupAttribute, AttributeType> primaryIndexSelector;

    public EntityMBigHuntBossGradeGroupAttributeTable(EntityMBigHuntBossGradeGroupAttribute[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AttributeType;
    }
}
