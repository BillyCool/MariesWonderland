using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpGradeTable : TableBase<EntityMPvpGrade>
{
    private readonly Func<EntityMPvpGrade, int> primaryIndexSelector;

    public EntityMPvpGradeTable(EntityMPvpGrade[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PvpGradeId;
    }
}
