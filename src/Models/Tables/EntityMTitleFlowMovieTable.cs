using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMTitleFlowMovieTable : TableBase<EntityMTitleFlowMovie>
{
    private readonly Func<EntityMTitleFlowMovie, int> primaryIndexSelector;

    public EntityMTitleFlowMovieTable(EntityMTitleFlowMovie[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.TitleFlowMovieId;
    }
}
