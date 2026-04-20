using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMReportTable : TableBase<EntityMReport>
{
    private readonly Func<EntityMReport, int> primaryIndexSelector;
    private readonly Func<EntityMReport, int> secondaryIndexSelector;

    public EntityMReportTable(EntityMReport[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ReportId;
        secondaryIndexSelector = element => element.MainQuestSeasonId;
    }

    public EntityMReport FindByReportId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
