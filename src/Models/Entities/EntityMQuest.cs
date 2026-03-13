using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuest
{
    public int QuestId { get; set; }

    public int NameQuestTextId { get; set; }

    public int PictureBookNameQuestTextId { get; set; }

    public int QuestReleaseConditionListId { get; set; }

    public int StoryQuestTextId { get; set; }

    public int QuestDisplayAttributeGroupId { get; set; }

    public int RecommendedDeckPower { get; set; }

    public int QuestFirstClearRewardGroupId { get; set; }

    public int QuestPickupRewardGroupId { get; set; }

    public int QuestDeckRestrictionGroupId { get; set; }

    public int QuestMissionGroupId { get; set; }

    public int Stamina { get; set; }

    public int UserExp { get; set; }

    public int CharacterExp { get; set; }

    public int CostumeExp { get; set; }

    public int Gold { get; set; }

    public int DailyClearableCount { get; set; }

    public bool IsRunInTheBackground { get; set; }

    public bool IsCountedAsQuest { get; set; }

    public int QuestBonusId { get; set; }

    public bool IsNotShowAfterClear { get; set; }

    public bool IsBigWinTarget { get; set; }

    public bool IsUsableSkipTicket { get; set; }

    public int QuestReplayFlowRewardGroupId { get; set; }

    public int InvisibleQuestMissionGroupId { get; set; }

    public int FieldEffectGroupId { get; set; }
}
