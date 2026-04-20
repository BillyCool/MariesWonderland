using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntBossQuestGroupChallengeCategory))]
public class EntityMBigHuntBossQuestGroupChallengeCategory
{
    [Key(0)] public int BigHuntBossQuestGroupChallengeCategoryId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int BigHuntBossQuestGroupId { get; set; }
}
