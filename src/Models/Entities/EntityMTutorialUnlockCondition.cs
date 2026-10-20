using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTutorialUnlockCondition))]
[PrimaryKey(nameof(TutorialType))]
public class EntityMTutorialUnlockCondition
{
    [Key(0)] public TutorialType TutorialType { get; set; }

    [Key(1)] public TutorialUnlockConditionType TutorialUnlockConditionType { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
