using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTitleFlowMovie
{
    public int TitleFlowMovieId { get; set; }

    public int MovieId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
