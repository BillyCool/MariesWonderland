using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTitleFlowMovie))]
public class EntityMTitleFlowMovie
{
    [Key(0)] public int TitleFlowMovieId { get; set; }

    [Key(1)] public int MovieId { get; set; }

    [Key(2)] public long StartDatetime { get; set; }

    [Key(3)] public long EndDatetime { get; set; }
}
