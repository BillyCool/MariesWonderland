using MariesWonderland.Models.Entities;

namespace MariesWonderland.Data;

/// <summary>
/// In-memory per-user database holding all client-visible (EntityI*) and server-only (EntityS*)
/// entity collections. One instance exists per logged-in user, keyed by userId in <see cref="UserDataStore"/>.
/// EntityI* properties are included in diff responses sent to the client; EntityS* properties are
/// server-side bookkeeping that is never transmitted.
/// </summary>
public class DarkUserMemoryDatabase
{
    public List<EntityIUser> EntityIUser { get; set; } = [];

    public List<EntityIUserApple> EntityIUserApple { get; set; } = [];

    public List<EntityIUserAutoSaleSettingDetail> EntityIUserAutoSaleSettingDetail { get; set; } = [];

    public List<EntityIUserBeginnerCampaign> EntityIUserBeginnerCampaign { get; set; } = [];

    public List<EntityIUserBigHuntMaxScore> EntityIUserBigHuntMaxScore { get; set; } = [];

    public List<EntityIUserBigHuntProgressStatus> EntityIUserBigHuntProgressStatus { get; set; } = [];

    public List<EntityIUserBigHuntScheduleMaxScore> EntityIUserBigHuntScheduleMaxScore { get; set; } = [];

    public List<EntityIUserBigHuntStatus> EntityIUserBigHuntStatus { get; set; } = [];

    public List<EntityIUserBigHuntWeeklyMaxScore> EntityIUserBigHuntWeeklyMaxScore { get; set; } = [];

    public List<EntityIUserBigHuntWeeklyStatus> EntityIUserBigHuntWeeklyStatus { get; set; } = [];

    public List<EntityIUserCageOrnamentReward> EntityIUserCageOrnamentReward { get; set; } = [];

    public List<EntityIUserCharacter> EntityIUserCharacter { get; set; } = [];

    public List<EntityIUserCharacterBoard> EntityIUserCharacterBoard { get; set; } = [];

    public List<EntityIUserCharacterBoardAbility> EntityIUserCharacterBoardAbility { get; set; } = [];

    public List<EntityIUserCharacterBoardCompleteReward> EntityIUserCharacterBoardCompleteReward { get; set; } = [];

    public List<EntityIUserCharacterBoardStatusUp> EntityIUserCharacterBoardStatusUp { get; set; } = [];

    public List<EntityIUserCharacterCostumeLevelBonus> EntityIUserCharacterCostumeLevelBonus { get; set; } = [];

    public List<EntityIUserCharacterRebirth> EntityIUserCharacterRebirth { get; set; } = [];

    public List<EntityIUserCharacterViewerField> EntityIUserCharacterViewerField { get; set; } = [];

    public List<EntityIUserComebackCampaign> EntityIUserComebackCampaign { get; set; } = [];

    public List<EntityIUserCompanion> EntityIUserCompanion { get; set; } = [];

    public List<EntityIUserConsumableItem> EntityIUserConsumableItem { get; set; } = [];

    public List<EntityIUserContentsStory> EntityIUserContentsStory { get; set; } = [];

    public List<EntityIUserCostume> EntityIUserCostume { get; set; } = [];

    public List<EntityIUserCostumeActiveSkill> EntityIUserCostumeActiveSkill { get; set; } = [];

    public List<EntityIUserCostumeAwakenStatusUp> EntityIUserCostumeAwakenStatusUp { get; set; } = [];

    public List<EntityIUserCostumeLevelBonusReleaseStatus> EntityIUserCostumeLevelBonusReleaseStatus { get; set; } = [];

    public List<EntityIUserCostumeLotteryEffect> EntityIUserCostumeLotteryEffect { get; set; } = [];

    public List<EntityIUserCostumeLotteryEffectAbility> EntityIUserCostumeLotteryEffectAbility { get; set; } = [];

    public List<EntityIUserCostumeLotteryEffectPending> EntityIUserCostumeLotteryEffectPending { get; set; } = [];

    public List<EntityIUserCostumeLotteryEffectStatusUp> EntityIUserCostumeLotteryEffectStatusUp { get; set; } = [];

    public List<EntityIUserDeck> EntityIUserDeck { get; set; } = [];

    public List<EntityIUserDeckCharacter> EntityIUserDeckCharacter { get; set; } = [];

    public List<EntityIUserDeckCharacterDressupCostume> EntityIUserDeckCharacterDressupCostume { get; set; } = [];

    public List<EntityIUserDeckLimitContentDeletedCharacter> EntityIUserDeckLimitContentDeletedCharacter { get; set; } = [];

    public List<EntityIUserDeckLimitContentRestricted> EntityIUserDeckLimitContentRestricted { get; set; } = [];

    public List<EntityIUserDeckPartsGroup> EntityIUserDeckPartsGroup { get; set; } = [];

    public List<EntityIUserDeckSubWeaponGroup> EntityIUserDeckSubWeaponGroup { get; set; } = [];

    public List<EntityIUserDeckTypeNote> EntityIUserDeckTypeNote { get; set; } = [];

    public List<EntityIUserDokan> EntityIUserDokan { get; set; } = [];

    public List<EntityIUserEventQuestDailyGroupCompleteReward> EntityIUserEventQuestDailyGroupCompleteReward { get; set; } = [];

    public List<EntityIUserEventQuestGuerrillaFreeOpen> EntityIUserEventQuestGuerrillaFreeOpen { get; set; } = [];

    public List<EntityIUserEventQuestLabyrinthSeason> EntityIUserEventQuestLabyrinthSeason { get; set; } = [];

    public List<EntityIUserEventQuestLabyrinthStage> EntityIUserEventQuestLabyrinthStage { get; set; } = [];

    public List<EntityIUserEventQuestProgressStatus> EntityIUserEventQuestProgressStatus { get; set; } = [];

    public List<EntityIUserEventQuestTowerAccumulationReward> EntityIUserEventQuestTowerAccumulationReward { get; set; } = [];

    public List<EntityIUserExplore> EntityIUserExplore { get; set; } = [];

    public List<EntityIUserExploreScore> EntityIUserExploreScore { get; set; } = [];

    public List<EntityIUserExtraQuestProgressStatus> EntityIUserExtraQuestProgressStatus { get; set; } = [];

    public List<EntityIUserFacebook> EntityIUserFacebook { get; set; } = [];

    public List<EntityIUserGem> EntityIUserGem { get; set; } = [];

    public List<EntitySUserGift> EntitySUserGift { get; set; } = [];

    public List<EntityIUserGimmick> EntityIUserGimmick { get; set; } = [];

    public List<EntityIUserGimmickOrnamentProgress> EntityIUserGimmickOrnamentProgress { get; set; } = [];

    public List<EntityIUserGimmickSequence> EntityIUserGimmickSequence { get; set; } = [];

    public List<EntityIUserGimmickUnlock> EntityIUserGimmickUnlock { get; set; } = [];

    public List<EntityIUserImportantItem> EntityIUserImportantItem { get; set; } = [];

    public List<EntityIUserLimitedOpen> EntityIUserLimitedOpen { get; set; } = [];

    public List<EntityIUserLogin> EntityIUserLogin { get; set; } = [];

    public List<EntityIUserLoginBonus> EntityIUserLoginBonus { get; set; } = [];

    public List<EntityIUserMainQuestFlowStatus> EntityIUserMainQuestFlowStatus { get; set; } = [];

    public List<EntityIUserMainQuestMainFlowStatus> EntityIUserMainQuestMainFlowStatus { get; set; } = [];

    public List<EntityIUserMainQuestProgressStatus> EntityIUserMainQuestProgressStatus { get; set; } = [];

    public List<EntityIUserMainQuestReplayFlowStatus> EntityIUserMainQuestReplayFlowStatus { get; set; } = [];

    public List<EntityIUserMainQuestSeasonRoute> EntityIUserMainQuestSeasonRoute { get; set; } = [];

    public List<EntityIUserMaterial> EntityIUserMaterial { get; set; } = [];

    public List<EntityIUserMission> EntityIUserMission { get; set; } = [];

    public List<EntityIUserMissionCompletionProgress> EntityIUserMissionCompletionProgress { get; set; } = [];

    public List<EntityIUserMissionPassPoint> EntityIUserMissionPassPoint { get; set; } = [];

    public List<EntityIUserMovie> EntityIUserMovie { get; set; } = [];

    public List<EntityIUserNaviCutIn> EntityIUserNaviCutIn { get; set; } = [];

    public List<EntityIUserOmikuji> EntityIUserOmikuji { get; set; } = [];

    public List<EntityIUserParts> EntityIUserParts { get; set; } = [];

    public List<EntityIUserPartsGroupNote> EntityIUserPartsGroupNote { get; set; } = [];

    public List<EntityIUserPartsPreset> EntityIUserPartsPreset { get; set; } = [];

    public List<EntityIUserPartsPresetTag> EntityIUserPartsPresetTag { get; set; } = [];

    public List<EntityIUserPartsStatusSub> EntityIUserPartsStatusSub { get; set; } = [];

    public List<EntityIUserPortalCageStatus> EntityIUserPortalCageStatus { get; set; } = [];

    public List<EntityIUserPossessionAutoConvert> EntityIUserPossessionAutoConvert { get; set; } = [];

    public List<EntityIUserPremiumItem> EntityIUserPremiumItem { get; set; } = [];

    public List<EntityIUserProfile> EntityIUserProfile { get; set; } = [];

    public List<EntityIUserPvpDefenseDeck> EntityIUserPvpDefenseDeck { get; set; } = [];

    public List<EntityIUserPvpStatus> EntityIUserPvpStatus { get; set; } = [];

    public List<EntityIUserPvpWeeklyResult> EntityIUserPvpWeeklyResult { get; set; } = [];

    public List<EntityIUserQuest> EntityIUserQuest { get; set; } = [];

    public List<EntityIUserQuestAutoOrbit> EntityIUserQuestAutoOrbit { get; set; } = [];

    public List<EntityIUserQuestLimitContentStatus> EntityIUserQuestLimitContentStatus { get; set; } = [];

    public List<EntityIUserQuestMission> EntityIUserQuestMission { get; set; } = [];

    public List<EntityIUserQuestReplayFlowRewardGroup> EntityIUserQuestReplayFlowRewardGroup { get; set; } = [];

    public List<EntityIUserQuestSceneChoice> EntityIUserQuestSceneChoice { get; set; } = [];

    public List<EntityIUserQuestSceneChoiceHistory> EntityIUserQuestSceneChoiceHistory { get; set; } = [];

    public List<EntityIUserSetting> EntityIUserSetting { get; set; } = [];

    public List<EntityIUserShopItem> EntityIUserShopItem { get; set; } = [];

    public List<EntityIUserShopReplaceable> EntityIUserShopReplaceable { get; set; } = [];

    public List<EntityIUserShopReplaceableLineup> EntityIUserShopReplaceableLineup { get; set; } = [];

    public List<EntityIUserSideStoryQuest> EntityIUserSideStoryQuest { get; set; } = [];

    public List<EntityIUserSideStoryQuestSceneProgressStatus> EntityIUserSideStoryQuestSceneProgressStatus { get; set; } = [];

    public List<EntityIUserStatus> EntityIUserStatus { get; set; } = [];

    public List<EntityIUserThought> EntityIUserThought { get; set; } = [];

    public List<EntityIUserTripleDeck> EntityIUserTripleDeck { get; set; } = [];

    public List<EntityIUserTutorialProgress> EntityIUserTutorialProgress { get; set; } = [];

    public List<EntityIUserWeapon> EntityIUserWeapon { get; set; } = [];

    public List<EntityIUserWeaponAbility> EntityIUserWeaponAbility { get; set; } = [];

    public List<EntityIUserWeaponAwaken> EntityIUserWeaponAwaken { get; set; } = [];

    public List<EntityIUserWeaponNote> EntityIUserWeaponNote { get; set; } = [];

    public List<EntityIUserWeaponSkill> EntityIUserWeaponSkill { get; set; } = [];

    public List<EntityIUserWeaponStory> EntityIUserWeaponStory { get; set; } = [];

    public List<EntityIUserWebviewPanelMission> EntityIUserWebviewPanelMission { get; set; } = [];

    // Server-exclusive data (EntityS* prefix): never sent to client
    public List<EntitySUser> EntitySUser { get; set; } = [];

    public List<EntitySUserDevice> EntitySUserDevice { get; set; } = [];

    public List<EntitySBattleDetail> BattleDetails { get; set; } = [];

    public List<EntitySQuestSession> EntitySQuestSession { get; set; } = [];

    public List<EntitySBigHuntSession> EntitySBigHuntSession { get; set; } = [];

    public List<EntitySGachaBannerState> EntitySGachaBannerState { get; set; } = [];

    public List<EntitySGachaRewardState> EntitySGachaRewardState { get; set; } = [];

}
