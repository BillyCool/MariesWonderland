using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpSeasonGradeTable : TableBase<EntityMPvpSeasonGrade>
{
    private readonly Func<EntityMPvpSeasonGrade, (int, int)> primaryIndexSelector;

    public EntityMPvpSeasonGradeTable(EntityMPvpSeasonGrade[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PvpGradeId, element.PvpSeasonId);
    }
}
