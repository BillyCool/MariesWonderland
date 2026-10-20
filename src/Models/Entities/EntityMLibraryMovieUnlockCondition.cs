using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLibraryMovieUnlockCondition))]
[PrimaryKey(nameof(LibraryMovieUnlockConditionId))]
public class EntityMLibraryMovieUnlockCondition
{
    [Key(0)] public int LibraryMovieUnlockConditionId { get; set; }

    [Key(1)] public UnlockConditionType UnlockConditionType { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
