using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLimitContent))]
public class EntityMEventQuestLimitContent
{
    [Key(0)] public int EventQuestLimitContentId { get; set; }

    [Key(1)] public int CostumeId { get; set; }

    [Key(2)] public int UnlockEvaluateConditionId { get; set; }

    [Key(3)] public int SortOrder { get; set; }

    [Key(4)] public int DeckGroupNumber { get; set; }

    [Key(5)] public long StartDatetime { get; set; }

    [Key(6)] public long EndDatetime { get; set; }

    [Key(7)] public int EventQuestLimitContentDeckRestrictionId { get; set; }
}
