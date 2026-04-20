using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuest))]
public class EntityMQuest
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int NameQuestTextId { get; set; }

    [Key(2)] public int PictureBookNameQuestTextId { get; set; }

    [Key(3)] public int QuestReleaseConditionListId { get; set; }

    [Key(4)] public int StoryQuestTextId { get; set; }

    [Key(5)] public int QuestDisplayAttributeGroupId { get; set; }

    [Key(6)] public int RecommendedDeckPower { get; set; }

    [Key(7)] public int QuestFirstClearRewardGroupId { get; set; }

    [Key(8)] public int QuestPickupRewardGroupId { get; set; }

    [Key(9)] public int QuestDeckRestrictionGroupId { get; set; }

    [Key(10)] public int QuestMissionGroupId { get; set; }

    [Key(11)] public int Stamina { get; set; }

    [Key(12)] public int UserExp { get; set; }

    [Key(13)] public int CharacterExp { get; set; }

    [Key(14)] public int CostumeExp { get; set; }

    [Key(15)] public int Gold { get; set; }

    [Key(16)] public int DailyClearableCount { get; set; }

    [Key(17)] public bool IsRunInTheBackground { get; set; }

    [Key(18)] public bool IsCountedAsQuest { get; set; }

    [Key(19)] public int QuestBonusId { get; set; }

    [Key(20)] public bool IsNotShowAfterClear { get; set; }

    [Key(21)] public bool IsBigWinTarget { get; set; }

    [Key(22)] public bool IsUsableSkipTicket { get; set; }

    [Key(23)] public int QuestReplayFlowRewardGroupId { get; set; }

    [Key(24)] public int InvisibleQuestMissionGroupId { get; set; }

    [Key(25)] public int FieldEffectGroupId { get; set; }
}
