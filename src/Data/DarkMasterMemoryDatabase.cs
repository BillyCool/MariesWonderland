using MariesWonderland.Models.Entities;

namespace MariesWonderland.Data;

public class DarkMasterMemoryDatabase
{
    public List<EntityMAbility> EntityMAbility { get; set; } = [];

    public List<EntityMAbilityBehaviour> EntityMAbilityBehaviour { get; set; } = [];

    public List<EntityMAbilityBehaviourActionBless> EntityMAbilityBehaviourActionBless { get; set; } = [];

    public List<EntityMAbilityBehaviourActionPassiveSkill> EntityMAbilityBehaviourActionPassiveSkill { get; set; } = [];

    public List<EntityMAbilityBehaviourActionStatus> EntityMAbilityBehaviourActionStatus { get; set; } = [];

    public List<EntityMAbilityBehaviourActionStatusDown> EntityMAbilityBehaviourActionStatusDown { get; set; } = [];

    public List<EntityMAbilityBehaviourGroup> EntityMAbilityBehaviourGroup { get; set; } = [];

    public List<EntityMAbilityDetail> EntityMAbilityDetail { get; set; } = [];

    public List<EntityMAbilityLevelGroup> EntityMAbilityLevelGroup { get; set; } = [];

    public List<EntityMAbilityStatus> EntityMAbilityStatus { get; set; } = [];

    public List<EntityMActor> EntityMActor { get; set; } = [];

    public List<EntityMActorAnimation> EntityMActorAnimation { get; set; } = [];

    public List<EntityMActorAnimationCategory> EntityMActorAnimationCategory { get; set; } = [];

    public List<EntityMActorAnimationController> EntityMActorAnimationController { get; set; } = [];

    public List<EntityMActorObject> EntityMActorObject { get; set; } = [];

    public List<EntityMAppealDialog> EntityMAppealDialog { get; set; } = [];

    public List<EntityMAssetBackground> EntityMAssetBackground { get; set; } = [];

    public List<EntityMAssetCalculator> EntityMAssetCalculator { get; set; } = [];

    public List<EntityMAssetDataSetting> EntityMAssetDataSetting { get; set; } = [];

    public List<EntityMAssetEffect> EntityMAssetEffect { get; set; } = [];

    public List<EntityMAssetGradeIcon> EntityMAssetGradeIcon { get; set; } = [];

    public List<EntityMAssetTimeline> EntityMAssetTimeline { get; set; } = [];

    public List<EntityMAssetTurnbattlePrefab> EntityMAssetTurnbattlePrefab { get; set; } = [];

    public List<EntityMBattle> EntityMBattle { get; set; } = [];

    public List<EntityMBattleActorAi> EntityMBattleActorAi { get; set; } = [];

    public List<EntityMBattleActorSkillAiGroup> EntityMBattleActorSkillAiGroup { get; set; } = [];

    public List<EntityMBattleAdditionalAbility> EntityMBattleAdditionalAbility { get; set; } = [];

    public List<EntityMBattleAttributeDamageCoefficientDefine> EntityMBattleAttributeDamageCoefficientDefine { get; set; } = [];

    public List<EntityMBattleAttributeDamageCoefficientGroup> EntityMBattleAttributeDamageCoefficientGroup { get; set; } = [];

    public List<EntityMBattleBgmSet> EntityMBattleBgmSet { get; set; } = [];

    public List<EntityMBattleBgmSetGroup> EntityMBattleBgmSetGroup { get; set; } = [];

    public List<EntityMBattleBigHunt> EntityMBattleBigHunt { get; set; } = [];

    public List<EntityMBattleBigHuntDamageThresholdGroup> EntityMBattleBigHuntDamageThresholdGroup { get; set; } = [];

    public List<EntityMBattleBigHuntKnockDownGaugeValueConfigGroup> EntityMBattleBigHuntKnockDownGaugeValueConfigGroup { get; set; } = [];

    public List<EntityMBattleBigHuntPhaseGroup> EntityMBattleBigHuntPhaseGroup { get; set; } = [];

    public List<EntityMBattleCompanionSkillAiGroup> EntityMBattleCompanionSkillAiGroup { get; set; } = [];

    public List<EntityMBattleCostumeSkillFireAct> EntityMBattleCostumeSkillFireAct { get; set; } = [];

    public List<EntityMBattleCostumeSkillSe> EntityMBattleCostumeSkillSe { get; set; } = [];

    public List<EntityMBattleDropReward> EntityMBattleDropReward { get; set; } = [];

    public List<EntityMBattleEnemySizeTypeConfig> EntityMBattleEnemySizeTypeConfig { get; set; } = [];

    public List<EntityMBattleEvent> EntityMBattleEvent { get; set; } = [];

    public List<EntityMBattleEventGroup> EntityMBattleEventGroup { get; set; } = [];

    public List<EntityMBattleEventReceiverBehaviourGroup> EntityMBattleEventReceiverBehaviourGroup { get; set; } = [];

    public List<EntityMBattleEventReceiverBehaviourHudActSequence> EntityMBattleEventReceiverBehaviourHudActSequence { get; set; } = [];

    public List<EntityMBattleEventReceiverBehaviourRadioMessage> EntityMBattleEventReceiverBehaviourRadioMessage { get; set; } = [];

    public List<EntityMBattleEventTriggerBehaviourBattleStart> EntityMBattleEventTriggerBehaviourBattleStart { get; set; } = [];

    public List<EntityMBattleEventTriggerBehaviourGroup> EntityMBattleEventTriggerBehaviourGroup { get; set; } = [];

    public List<EntityMBattleEventTriggerBehaviourWaveStart> EntityMBattleEventTriggerBehaviourWaveStart { get; set; } = [];

    public List<EntityMBattleGeneralViewConfiguration> EntityMBattleGeneralViewConfiguration { get; set; } = [];

    public List<EntityMBattleGroup> EntityMBattleGroup { get; set; } = [];

    public List<EntityMBattleNpc> EntityMBattleNpc { get; set; } = [];

    public List<EntityMBattleNpcCharacter> EntityMBattleNpcCharacter { get; set; } = [];

    public List<EntityMBattleNpcCharacterBoard> EntityMBattleNpcCharacterBoard { get; set; } = [];

    public List<EntityMBattleNpcCharacterBoardAbility> EntityMBattleNpcCharacterBoardAbility { get; set; } = [];

    public List<EntityMBattleNpcCharacterBoardCompleteReward> EntityMBattleNpcCharacterBoardCompleteReward { get; set; } = [];

    public List<EntityMBattleNpcCharacterBoardStatusUp> EntityMBattleNpcCharacterBoardStatusUp { get; set; } = [];

    public List<EntityMBattleNpcCharacterCostumeLevelBonus> EntityMBattleNpcCharacterCostumeLevelBonus { get; set; } = [];

    public List<EntityMBattleNpcCharacterRebirth> EntityMBattleNpcCharacterRebirth { get; set; } = [];

    public List<EntityMBattleNpcCharacterViewerField> EntityMBattleNpcCharacterViewerField { get; set; } = [];

    public List<EntityMBattleNpcCompanion> EntityMBattleNpcCompanion { get; set; } = [];

    public List<EntityMBattleNpcCostume> EntityMBattleNpcCostume { get; set; } = [];

    public List<EntityMBattleNpcCostumeActiveSkill> EntityMBattleNpcCostumeActiveSkill { get; set; } = [];

    public List<EntityMBattleNpcCostumeAwakenStatusUp> EntityMBattleNpcCostumeAwakenStatusUp { get; set; } = [];

    public List<EntityMBattleNpcCostumeLevelBonusReevaluate> EntityMBattleNpcCostumeLevelBonusReevaluate { get; set; } = [];

    public List<EntityMBattleNpcCostumeLevelBonusReleaseStatus> EntityMBattleNpcCostumeLevelBonusReleaseStatus { get; set; } = [];

    public List<EntityMBattleNpcCostumeLotteryEffect> EntityMBattleNpcCostumeLotteryEffect { get; set; } = [];

    public List<EntityMBattleNpcCostumeLotteryEffectAbility> EntityMBattleNpcCostumeLotteryEffectAbility { get; set; } = [];

    public List<EntityMBattleNpcCostumeLotteryEffectPending> EntityMBattleNpcCostumeLotteryEffectPending { get; set; } = [];

    public List<EntityMBattleNpcCostumeLotteryEffectStatusUp> EntityMBattleNpcCostumeLotteryEffectStatusUp { get; set; } = [];

    public List<EntityMBattleNpcDeck> EntityMBattleNpcDeck { get; set; } = [];

    public List<EntityMBattleNpcDeckBackup> EntityMBattleNpcDeckBackup { get; set; } = [];

    public List<EntityMBattleNpcDeckCharacter> EntityMBattleNpcDeckCharacter { get; set; } = [];

    public List<EntityMBattleNpcDeckCharacterDressupCostume> EntityMBattleNpcDeckCharacterDressupCostume { get; set; } = [];

    public List<EntityMBattleNpcDeckCharacterDropCategory> EntityMBattleNpcDeckCharacterDropCategory { get; set; } = [];

    public List<EntityMBattleNpcDeckCharacterType> EntityMBattleNpcDeckCharacterType { get; set; } = [];

    public List<EntityMBattleNpcDeckLimitContentBackup> EntityMBattleNpcDeckLimitContentBackup { get; set; } = [];

    public List<EntityMBattleNpcDeckLimitContentBackupRestored> EntityMBattleNpcDeckLimitContentBackupRestored { get; set; } = [];

    public List<EntityMBattleNpcDeckLimitContentDeletedCharacter> EntityMBattleNpcDeckLimitContentDeletedCharacter { get; set; } = [];

    public List<EntityMBattleNpcDeckLimitContentRestricted> EntityMBattleNpcDeckLimitContentRestricted { get; set; } = [];

    public List<EntityMBattleNpcDeckPartsGroup> EntityMBattleNpcDeckPartsGroup { get; set; } = [];

    public List<EntityMBattleNpcDeckSubWeaponGroup> EntityMBattleNpcDeckSubWeaponGroup { get; set; } = [];

    public List<EntityMBattleNpcDeckTypeNote> EntityMBattleNpcDeckTypeNote { get; set; } = [];

    public List<EntityMBattleNpcParts> EntityMBattleNpcParts { get; set; } = [];

    public List<EntityMBattleNpcPartsGroupNote> EntityMBattleNpcPartsGroupNote { get; set; } = [];

    public List<EntityMBattleNpcPartsPreset> EntityMBattleNpcPartsPreset { get; set; } = [];

    public List<EntityMBattleNpcPartsPresetTag> EntityMBattleNpcPartsPresetTag { get; set; } = [];

    public List<EntityMBattleNpcPartsStatusSub> EntityMBattleNpcPartsStatusSub { get; set; } = [];

    public List<EntityMBattleNpcSpecialEndAct> EntityMBattleNpcSpecialEndAct { get; set; } = [];

    public List<EntityMBattleNpcWeapon> EntityMBattleNpcWeapon { get; set; } = [];

    public List<EntityMBattleNpcWeaponAbility> EntityMBattleNpcWeaponAbility { get; set; } = [];

    public List<EntityMBattleNpcWeaponAbilityReevaluate> EntityMBattleNpcWeaponAbilityReevaluate { get; set; } = [];

    public List<EntityMBattleNpcWeaponAwaken> EntityMBattleNpcWeaponAwaken { get; set; } = [];

    public List<EntityMBattleNpcWeaponNote> EntityMBattleNpcWeaponNote { get; set; } = [];

    public List<EntityMBattleNpcWeaponNoteReevaluate> EntityMBattleNpcWeaponNoteReevaluate { get; set; } = [];

    public List<EntityMBattleNpcWeaponSkill> EntityMBattleNpcWeaponSkill { get; set; } = [];

    public List<EntityMBattleNpcWeaponStory> EntityMBattleNpcWeaponStory { get; set; } = [];

    public List<EntityMBattleNpcWeaponStoryReevaluate> EntityMBattleNpcWeaponStoryReevaluate { get; set; } = [];

    public List<EntityMBattleProgressUiType> EntityMBattleProgressUiType { get; set; } = [];

    public List<EntityMBattleQuestSceneBgm> EntityMBattleQuestSceneBgm { get; set; } = [];

    public List<EntityMBattleQuestSceneBgmSetGroup> EntityMBattleQuestSceneBgmSetGroup { get; set; } = [];

    public List<EntityMBattleRentalDeck> EntityMBattleRentalDeck { get; set; } = [];

    public List<EntityMBattleSkillBehaviourHitDamageConfiguration> EntityMBattleSkillBehaviourHitDamageConfiguration { get; set; } = [];

    public List<EntityMBattleSkillFireAct> EntityMBattleSkillFireAct { get; set; } = [];

    public List<EntityMBattleSkillFireActConditionAttributeType> EntityMBattleSkillFireActConditionAttributeType { get; set; } = [];

    public List<EntityMBattleSkillFireActConditionGroup> EntityMBattleSkillFireActConditionGroup { get; set; } = [];

    public List<EntityMBattleSkillFireActConditionSkillCategoryType> EntityMBattleSkillFireActConditionSkillCategoryType { get; set; } = [];

    public List<EntityMBattleSkillFireActConditionWeaponType> EntityMBattleSkillFireActConditionWeaponType { get; set; } = [];

    public List<EntityMBeginnerCampaign> EntityMBeginnerCampaign { get; set; } = [];

    public List<EntityMBigHuntBoss> EntityMBigHuntBoss { get; set; } = [];

    public List<EntityMBigHuntBossGradeGroup> EntityMBigHuntBossGradeGroup { get; set; } = [];

    public List<EntityMBigHuntBossGradeGroupAttribute> EntityMBigHuntBossGradeGroupAttribute { get; set; } = [];

    public List<EntityMBigHuntBossQuest> EntityMBigHuntBossQuest { get; set; } = [];

    public List<EntityMBigHuntBossQuestGroup> EntityMBigHuntBossQuestGroup { get; set; } = [];

    public List<EntityMBigHuntBossQuestGroupChallengeCategory> EntityMBigHuntBossQuestGroupChallengeCategory { get; set; } = [];

    public List<EntityMBigHuntLink> EntityMBigHuntLink { get; set; } = [];

    public List<EntityMBigHuntQuest> EntityMBigHuntQuest { get; set; } = [];

    public List<EntityMBigHuntQuestGroup> EntityMBigHuntQuestGroup { get; set; } = [];

    public List<EntityMBigHuntQuestScoreCoefficient> EntityMBigHuntQuestScoreCoefficient { get; set; } = [];

    public List<EntityMBigHuntRewardGroup> EntityMBigHuntRewardGroup { get; set; } = [];

    public List<EntityMBigHuntSchedule> EntityMBigHuntSchedule { get; set; } = [];

    public List<EntityMBigHuntScoreRewardGroup> EntityMBigHuntScoreRewardGroup { get; set; } = [];

    public List<EntityMBigHuntScoreRewardGroupSchedule> EntityMBigHuntScoreRewardGroupSchedule { get; set; } = [];

    public List<EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule> EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule { get; set; } = [];

    public List<EntityMCageMemory> EntityMCageMemory { get; set; } = [];

    public List<EntityMCageOrnament> EntityMCageOrnament { get; set; } = [];

    public List<EntityMCageOrnamentMainQuestChapterStill> EntityMCageOrnamentMainQuestChapterStill { get; set; } = [];

    public List<EntityMCageOrnamentReward> EntityMCageOrnamentReward { get; set; } = [];

    public List<EntityMCageOrnamentStillReleaseCondition> EntityMCageOrnamentStillReleaseCondition { get; set; } = [];

    public List<EntityMCatalogCompanion> EntityMCatalogCompanion { get; set; } = [];

    public List<EntityMCatalogCostume> EntityMCatalogCostume { get; set; } = [];

    public List<EntityMCatalogPartsGroup> EntityMCatalogPartsGroup { get; set; } = [];

    public List<EntityMCatalogTerm> EntityMCatalogTerm { get; set; } = [];

    public List<EntityMCatalogThought> EntityMCatalogThought { get; set; } = [];

    public List<EntityMCatalogWeapon> EntityMCatalogWeapon { get; set; } = [];

    public List<EntityMCharacter> EntityMCharacter { get; set; } = [];

    public List<EntityMCharacterBoard> EntityMCharacterBoard { get; set; } = [];

    public List<EntityMCharacterBoardAbility> EntityMCharacterBoardAbility { get; set; } = [];

    public List<EntityMCharacterBoardAbilityMaxLevel> EntityMCharacterBoardAbilityMaxLevel { get; set; } = [];

    public List<EntityMCharacterBoardAssignment> EntityMCharacterBoardAssignment { get; set; } = [];

    public List<EntityMCharacterBoardCategory> EntityMCharacterBoardCategory { get; set; } = [];

    public List<EntityMCharacterBoardCompleteReward> EntityMCharacterBoardCompleteReward { get; set; } = [];

    public List<EntityMCharacterBoardCompleteRewardGroup> EntityMCharacterBoardCompleteRewardGroup { get; set; } = [];

    public List<EntityMCharacterBoardCondition> EntityMCharacterBoardCondition { get; set; } = [];

    public List<EntityMCharacterBoardConditionDetail> EntityMCharacterBoardConditionDetail { get; set; } = [];

    public List<EntityMCharacterBoardConditionGroup> EntityMCharacterBoardConditionGroup { get; set; } = [];

    public List<EntityMCharacterBoardConditionIgnore> EntityMCharacterBoardConditionIgnore { get; set; } = [];

    public List<EntityMCharacterBoardEffectTargetGroup> EntityMCharacterBoardEffectTargetGroup { get; set; } = [];

    public List<EntityMCharacterBoardGroup> EntityMCharacterBoardGroup { get; set; } = [];

    public List<EntityMCharacterBoardPanel> EntityMCharacterBoardPanel { get; set; } = [];

    public List<EntityMCharacterBoardPanelReleaseEffectGroup> EntityMCharacterBoardPanelReleaseEffectGroup { get; set; } = [];

    public List<EntityMCharacterBoardPanelReleasePossessionGroup> EntityMCharacterBoardPanelReleasePossessionGroup { get; set; } = [];

    public List<EntityMCharacterBoardPanelReleaseRewardGroup> EntityMCharacterBoardPanelReleaseRewardGroup { get; set; } = [];

    public List<EntityMCharacterBoardStatusUp> EntityMCharacterBoardStatusUp { get; set; } = [];

    public List<EntityMCharacterDisplaySwitch> EntityMCharacterDisplaySwitch { get; set; } = [];

    public List<EntityMCharacterLevelBonusAbilityGroup> EntityMCharacterLevelBonusAbilityGroup { get; set; } = [];

    public List<EntityMCharacterRebirth> EntityMCharacterRebirth { get; set; } = [];

    public List<EntityMCharacterRebirthMaterialGroup> EntityMCharacterRebirthMaterialGroup { get; set; } = [];

    public List<EntityMCharacterRebirthStepGroup> EntityMCharacterRebirthStepGroup { get; set; } = [];

    public List<EntityMCharacterViewerActorIcon> EntityMCharacterViewerActorIcon { get; set; } = [];

    public List<EntityMCharacterViewerField> EntityMCharacterViewerField { get; set; } = [];

    public List<EntityMCharacterViewerFieldSettings> EntityMCharacterViewerFieldSettings { get; set; } = [];

    public List<EntityMCharacterVoiceUnlockCondition> EntityMCharacterVoiceUnlockCondition { get; set; } = [];

    public List<EntityMCollectionBonusEffect> EntityMCollectionBonusEffect { get; set; } = [];

    public List<EntityMCollectionBonusQuestAssignment> EntityMCollectionBonusQuestAssignment { get; set; } = [];

    public List<EntityMCollectionBonusQuestAssignmentGroup> EntityMCollectionBonusQuestAssignmentGroup { get; set; } = [];

    public List<EntityMComboCalculationSetting> EntityMComboCalculationSetting { get; set; } = [];

    public List<EntityMComebackCampaign> EntityMComebackCampaign { get; set; } = [];

    public List<EntityMCompanion> EntityMCompanion { get; set; } = [];

    public List<EntityMCompanionAbilityGroup> EntityMCompanionAbilityGroup { get; set; } = [];

    public List<EntityMCompanionAbilityLevel> EntityMCompanionAbilityLevel { get; set; } = [];

    public List<EntityMCompanionBaseStatus> EntityMCompanionBaseStatus { get; set; } = [];

    public List<EntityMCompanionCategory> EntityMCompanionCategory { get; set; } = [];

    public List<EntityMCompanionDuplicationExchangePossessionGroup> EntityMCompanionDuplicationExchangePossessionGroup { get; set; } = [];

    public List<EntityMCompanionEnhanced> EntityMCompanionEnhanced { get; set; } = [];

    public List<EntityMCompanionEnhancementMaterial> EntityMCompanionEnhancementMaterial { get; set; } = [];

    public List<EntityMCompanionSkillLevel> EntityMCompanionSkillLevel { get; set; } = [];

    public List<EntityMCompanionStatusCalculation> EntityMCompanionStatusCalculation { get; set; } = [];

    public List<EntityMCompleteMissionGroup> EntityMCompleteMissionGroup { get; set; } = [];

    public List<EntityMConfig> EntityMConfig { get; set; } = [];

    public List<EntityMConsumableItem> EntityMConsumableItem { get; set; } = [];

    public List<EntityMConsumableItemEffect> EntityMConsumableItemEffect { get; set; } = [];

    public List<EntityMConsumableItemTerm> EntityMConsumableItemTerm { get; set; } = [];

    public List<EntityMContentsStory> EntityMContentsStory { get; set; } = [];

    public List<EntityMCostume> EntityMCostume { get; set; } = [];

    public List<EntityMCostumeAbilityGroup> EntityMCostumeAbilityGroup { get; set; } = [];

    public List<EntityMCostumeAbilityLevelGroup> EntityMCostumeAbilityLevelGroup { get; set; } = [];

    public List<EntityMCostumeActiveSkillEnhancementMaterial> EntityMCostumeActiveSkillEnhancementMaterial { get; set; } = [];

    public List<EntityMCostumeActiveSkillGroup> EntityMCostumeActiveSkillGroup { get; set; } = [];

    public List<EntityMCostumeAnimationStep> EntityMCostumeAnimationStep { get; set; } = [];

    public List<EntityMCostumeAutoOrganizationCondition> EntityMCostumeAutoOrganizationCondition { get; set; } = [];

    public List<EntityMCostumeAwaken> EntityMCostumeAwaken { get; set; } = [];

    public List<EntityMCostumeAwakenAbility> EntityMCostumeAwakenAbility { get; set; } = [];

    public List<EntityMCostumeAwakenEffectGroup> EntityMCostumeAwakenEffectGroup { get; set; } = [];

    public List<EntityMCostumeAwakenItemAcquire> EntityMCostumeAwakenItemAcquire { get; set; } = [];

    public List<EntityMCostumeAwakenMaterialGroup> EntityMCostumeAwakenMaterialGroup { get; set; } = [];

    public List<EntityMCostumeAwakenPriceGroup> EntityMCostumeAwakenPriceGroup { get; set; } = [];

    public List<EntityMCostumeAwakenStatusUpGroup> EntityMCostumeAwakenStatusUpGroup { get; set; } = [];

    public List<EntityMCostumeAwakenStepMaterialGroup> EntityMCostumeAwakenStepMaterialGroup { get; set; } = [];

    public List<EntityMCostumeBaseStatus> EntityMCostumeBaseStatus { get; set; } = [];

    public List<EntityMCostumeCollectionBonus> EntityMCostumeCollectionBonus { get; set; } = [];

    public List<EntityMCostumeCollectionBonusGroup> EntityMCostumeCollectionBonusGroup { get; set; } = [];

    public List<EntityMCostumeDefaultSkillGroup> EntityMCostumeDefaultSkillGroup { get; set; } = [];

    public List<EntityMCostumeDefaultSkillLotteryGroup> EntityMCostumeDefaultSkillLotteryGroup { get; set; } = [];

    public List<EntityMCostumeDelete> EntityMCostumeDelete { get; set; } = [];

    public List<EntityMCostumeDisplayCoordinateAdjustment> EntityMCostumeDisplayCoordinateAdjustment { get; set; } = [];

    public List<EntityMCostumeDisplaySwitch> EntityMCostumeDisplaySwitch { get; set; } = [];

    public List<EntityMCostumeDuplicationExchangePossessionGroup> EntityMCostumeDuplicationExchangePossessionGroup { get; set; } = [];

    public List<EntityMCostumeEmblem> EntityMCostumeEmblem { get; set; } = [];

    public List<EntityMCostumeEnhanced> EntityMCostumeEnhanced { get; set; } = [];

    public List<EntityMCostumeLevelBonus> EntityMCostumeLevelBonus { get; set; } = [];

    public List<EntityMCostumeLimitBreakMaterialGroup> EntityMCostumeLimitBreakMaterialGroup { get; set; } = [];

    public List<EntityMCostumeLimitBreakMaterialRarityGroup> EntityMCostumeLimitBreakMaterialRarityGroup { get; set; } = [];

    public List<EntityMCostumeLotteryEffect> EntityMCostumeLotteryEffect { get; set; } = [];

    public List<EntityMCostumeLotteryEffectMaterialGroup> EntityMCostumeLotteryEffectMaterialGroup { get; set; } = [];

    public List<EntityMCostumeLotteryEffectOddsGroup> EntityMCostumeLotteryEffectOddsGroup { get; set; } = [];

    public List<EntityMCostumeLotteryEffectReleaseSchedule> EntityMCostumeLotteryEffectReleaseSchedule { get; set; } = [];

    public List<EntityMCostumeLotteryEffectTargetAbility> EntityMCostumeLotteryEffectTargetAbility { get; set; } = [];

    public List<EntityMCostumeLotteryEffectTargetStatusUp> EntityMCostumeLotteryEffectTargetStatusUp { get; set; } = [];

    public List<EntityMCostumeOverflowExchangePossessionGroup> EntityMCostumeOverflowExchangePossessionGroup { get; set; } = [];

    public List<EntityMCostumeProperAttributeHpBonus> EntityMCostumeProperAttributeHpBonus { get; set; } = [];

    public List<EntityMCostumeRarity> EntityMCostumeRarity { get; set; } = [];

    public List<EntityMCostumeSpecialActActiveSkill> EntityMCostumeSpecialActActiveSkill { get; set; } = [];

    public List<EntityMCostumeSpecialActActiveSkillConditionAttribute> EntityMCostumeSpecialActActiveSkillConditionAttribute { get; set; } = [];

    public List<EntityMCostumeStatusCalculation> EntityMCostumeStatusCalculation { get; set; } = [];

    public List<EntityMDeckEntrustCoefficientAttribute> EntityMDeckEntrustCoefficientAttribute { get; set; } = [];

    public List<EntityMDeckEntrustCoefficientPartsSeriesBonusCount> EntityMDeckEntrustCoefficientPartsSeriesBonusCount { get; set; } = [];

    public List<EntityMDeckEntrustCoefficientStatus> EntityMDeckEntrustCoefficientStatus { get; set; } = [];

    public List<EntityMDokan> EntityMDokan { get; set; } = [];

    public List<EntityMDokanContentGroup> EntityMDokanContentGroup { get; set; } = [];

    public List<EntityMDokanText> EntityMDokanText { get; set; } = [];

    public List<EntityMEnhanceCampaign> EntityMEnhanceCampaign { get; set; } = [];

    public List<EntityMEnhanceCampaignTargetGroup> EntityMEnhanceCampaignTargetGroup { get; set; } = [];

    public List<EntityMEvaluateCondition> EntityMEvaluateCondition { get; set; } = [];

    public List<EntityMEvaluateConditionValueGroup> EntityMEvaluateConditionValueGroup { get; set; } = [];

    public List<EntityMEventQuestChapter> EntityMEventQuestChapter { get; set; } = [];

    public List<EntityMEventQuestChapterCharacter> EntityMEventQuestChapterCharacter { get; set; } = [];

    public List<EntityMEventQuestChapterDifficultyLimitContentUnlock> EntityMEventQuestChapterDifficultyLimitContentUnlock { get; set; } = [];

    public List<EntityMEventQuestChapterLimitContentRelation> EntityMEventQuestChapterLimitContentRelation { get; set; } = [];

    public List<EntityMEventQuestDailyGroup> EntityMEventQuestDailyGroup { get; set; } = [];

    public List<EntityMEventQuestDailyGroupCompleteReward> EntityMEventQuestDailyGroupCompleteReward { get; set; } = [];

    public List<EntityMEventQuestDailyGroupMessage> EntityMEventQuestDailyGroupMessage { get; set; } = [];

    public List<EntityMEventQuestDailyGroupTargetChapter> EntityMEventQuestDailyGroupTargetChapter { get; set; } = [];

    public List<EntityMEventQuestDisplayItemGroup> EntityMEventQuestDisplayItemGroup { get; set; } = [];

    public List<EntityMEventQuestGuerrillaFreeOpen> EntityMEventQuestGuerrillaFreeOpen { get; set; } = [];

    public List<EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondence> EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondence { get; set; } = [];

    public List<EntityMEventQuestLabyrinthMob> EntityMEventQuestLabyrinthMob { get; set; } = [];

    public List<EntityMEventQuestLabyrinthQuestDisplay> EntityMEventQuestLabyrinthQuestDisplay { get; set; } = [];

    public List<EntityMEventQuestLabyrinthQuestEffectDescriptionAbility> EntityMEventQuestLabyrinthQuestEffectDescriptionAbility { get; set; } = [];

    public List<EntityMEventQuestLabyrinthQuestEffectDescriptionFree> EntityMEventQuestLabyrinthQuestEffectDescriptionFree { get; set; } = [];

    public List<EntityMEventQuestLabyrinthQuestEffectDisplay> EntityMEventQuestLabyrinthQuestEffectDisplay { get; set; } = [];

    public List<EntityMEventQuestLabyrinthRewardGroup> EntityMEventQuestLabyrinthRewardGroup { get; set; } = [];

    public List<EntityMEventQuestLabyrinthSeason> EntityMEventQuestLabyrinthSeason { get; set; } = [];

    public List<EntityMEventQuestLabyrinthSeasonRewardGroup> EntityMEventQuestLabyrinthSeasonRewardGroup { get; set; } = [];

    public List<EntityMEventQuestLabyrinthStage> EntityMEventQuestLabyrinthStage { get; set; } = [];

    public List<EntityMEventQuestLabyrinthStageAccumulationRewardGroup> EntityMEventQuestLabyrinthStageAccumulationRewardGroup { get; set; } = [];

    public List<EntityMEventQuestLimitContent> EntityMEventQuestLimitContent { get; set; } = [];

    public List<EntityMEventQuestLimitContentDeckRestriction> EntityMEventQuestLimitContentDeckRestriction { get; set; } = [];

    public List<EntityMEventQuestLimitContentDeckRestrictionTarget> EntityMEventQuestLimitContentDeckRestrictionTarget { get; set; } = [];

    public List<EntityMEventQuestLink> EntityMEventQuestLink { get; set; } = [];

    public List<EntityMEventQuestSequence> EntityMEventQuestSequence { get; set; } = [];

    public List<EntityMEventQuestSequenceGroup> EntityMEventQuestSequenceGroup { get; set; } = [];

    public List<EntityMEventQuestTowerAccumulationReward> EntityMEventQuestTowerAccumulationReward { get; set; } = [];

    public List<EntityMEventQuestTowerAccumulationRewardGroup> EntityMEventQuestTowerAccumulationRewardGroup { get; set; } = [];

    public List<EntityMEventQuestTowerAsset> EntityMEventQuestTowerAsset { get; set; } = [];

    public List<EntityMEventQuestTowerRewardGroup> EntityMEventQuestTowerRewardGroup { get; set; } = [];

    public List<EntityMEventQuestUnlockCondition> EntityMEventQuestUnlockCondition { get; set; } = [];

    public List<EntityMExplore> EntityMExplore { get; set; } = [];

    public List<EntityMExploreGradeAsset> EntityMExploreGradeAsset { get; set; } = [];

    public List<EntityMExploreGradeScore> EntityMExploreGradeScore { get; set; } = [];

    public List<EntityMExploreGroup> EntityMExploreGroup { get; set; } = [];

    public List<EntityMExploreUnlockCondition> EntityMExploreUnlockCondition { get; set; } = [];

    public List<EntityMExtraQuestGroup> EntityMExtraQuestGroup { get; set; } = [];

    public List<EntityMExtraQuestGroupInMainQuestChapter> EntityMExtraQuestGroupInMainQuestChapter { get; set; } = [];

    public List<EntityMFieldEffectBlessRelation> EntityMFieldEffectBlessRelation { get; set; } = [];

    public List<EntityMFieldEffectDecreasePoint> EntityMFieldEffectDecreasePoint { get; set; } = [];

    public List<EntityMFieldEffectGroup> EntityMFieldEffectGroup { get; set; } = [];

    public List<EntityMGachaMedal> EntityMGachaMedal { get; set; } = [];

    public List<EntityMGiftText> EntityMGiftText { get; set; } = [];

    public List<EntityMGimmick> EntityMGimmick { get; set; } = [];

    public List<EntityMGimmickAdditionalAsset> EntityMGimmickAdditionalAsset { get; set; } = [];

    public List<EntityMGimmickExtraQuest> EntityMGimmickExtraQuest { get; set; } = [];

    public List<EntityMGimmickGroup> EntityMGimmickGroup { get; set; } = [];

    public List<EntityMGimmickGroupEventLog> EntityMGimmickGroupEventLog { get; set; } = [];

    public List<EntityMGimmickInterval> EntityMGimmickInterval { get; set; } = [];

    public List<EntityMGimmickOrnament> EntityMGimmickOrnament { get; set; } = [];

    public List<EntityMGimmickSequence> EntityMGimmickSequence { get; set; } = [];

    public List<EntityMGimmickSequenceGroup> EntityMGimmickSequenceGroup { get; set; } = [];

    public List<EntityMGimmickSequenceRewardGroup> EntityMGimmickSequenceRewardGroup { get; set; } = [];

    public List<EntityMGimmickSequenceSchedule> EntityMGimmickSequenceSchedule { get; set; } = [];

    public List<EntityMHeadupDisplayView> EntityMHeadupDisplayView { get; set; } = [];

    public List<EntityMHelp> EntityMHelp { get; set; } = [];

    public List<EntityMHelpCategory> EntityMHelpCategory { get; set; } = [];

    public List<EntityMHelpItem> EntityMHelpItem { get; set; } = [];

    public List<EntityMHelpPageGroup> EntityMHelpPageGroup { get; set; } = [];

    public List<EntityMImportantItem> EntityMImportantItem { get; set; } = [];

    public List<EntityMImportantItemEffect> EntityMImportantItemEffect { get; set; } = [];

    public List<EntityMImportantItemEffectDropCount> EntityMImportantItemEffectDropCount { get; set; } = [];

    public List<EntityMImportantItemEffectDropRate> EntityMImportantItemEffectDropRate { get; set; } = [];

    public List<EntityMImportantItemEffectTargetItemGroup> EntityMImportantItemEffectTargetItemGroup { get; set; } = [];

    public List<EntityMImportantItemEffectTargetQuestGroup> EntityMImportantItemEffectTargetQuestGroup { get; set; } = [];

    public List<EntityMImportantItemEffectUnlockFunction> EntityMImportantItemEffectUnlockFunction { get; set; } = [];

    public List<EntityMLibraryEventQuestStoryGrouping> EntityMLibraryEventQuestStoryGrouping { get; set; } = [];

    public List<EntityMLibraryMainQuestGroup> EntityMLibraryMainQuestGroup { get; set; } = [];

    public List<EntityMLibraryMainQuestStory> EntityMLibraryMainQuestStory { get; set; } = [];

    public List<EntityMLibraryMovie> EntityMLibraryMovie { get; set; } = [];

    public List<EntityMLibraryMovieCategory> EntityMLibraryMovieCategory { get; set; } = [];

    public List<EntityMLibraryMovieUnlockCondition> EntityMLibraryMovieUnlockCondition { get; set; } = [];

    public List<EntityMLibraryRecordGrouping> EntityMLibraryRecordGrouping { get; set; } = [];

    public List<EntityMLibraryStoryGroup> EntityMLibraryStoryGroup { get; set; } = [];

    public List<EntityMLimitedOpenText> EntityMLimitedOpenText { get; set; } = [];

    public List<EntityMLimitedOpenTextGroup> EntityMLimitedOpenTextGroup { get; set; } = [];

    public List<EntityMListSettingAbilityGroup> EntityMListSettingAbilityGroup { get; set; } = [];

    public List<EntityMListSettingAbilityGroupTarget> EntityMListSettingAbilityGroupTarget { get; set; } = [];

    public List<EntityMLoginBonus> EntityMLoginBonus { get; set; } = [];

    public List<EntityMLoginBonusStamp> EntityMLoginBonusStamp { get; set; } = [];

    public List<EntityMMainQuestChapter> EntityMMainQuestChapter { get; set; } = [];

    public List<EntityMMainQuestPortalCageCharacter> EntityMMainQuestPortalCageCharacter { get; set; } = [];

    public List<EntityMMainQuestRoute> EntityMMainQuestRoute { get; set; } = [];

    public List<EntityMMainQuestRouteAnotherReplayFlowUnlockCondition> EntityMMainQuestRouteAnotherReplayFlowUnlockCondition { get; set; } = [];

    public List<EntityMMainQuestSeason> EntityMMainQuestSeason { get; set; } = [];

    public List<EntityMMainQuestSequence> EntityMMainQuestSequence { get; set; } = [];

    public List<EntityMMainQuestSequenceGroup> EntityMMainQuestSequenceGroup { get; set; } = [];

    public List<EntityMMaintenance> EntityMMaintenance { get; set; } = [];

    public List<EntityMMaintenanceGroup> EntityMMaintenanceGroup { get; set; } = [];

    public List<EntityMMaterial> EntityMMaterial { get; set; } = [];

    public List<EntityMMaterialSaleObtainPossession> EntityMMaterialSaleObtainPossession { get; set; } = [];

    public List<EntityMMission> EntityMMission { get; set; } = [];

    public List<EntityMMissionClearConditionValueView> EntityMMissionClearConditionValueView { get; set; } = [];

    public List<EntityMMissionGroup> EntityMMissionGroup { get; set; } = [];

    public List<EntityMMissionLink> EntityMMissionLink { get; set; } = [];

    public List<EntityMMissionPass> EntityMMissionPass { get; set; } = [];

    public List<EntityMMissionPassLevelGroup> EntityMMissionPassLevelGroup { get; set; } = [];

    public List<EntityMMissionPassMissionGroup> EntityMMissionPassMissionGroup { get; set; } = [];

    public List<EntityMMissionPassRewardGroup> EntityMMissionPassRewardGroup { get; set; } = [];

    public List<EntityMMissionReward> EntityMMissionReward { get; set; } = [];

    public List<EntityMMissionSubCategoryText> EntityMMissionSubCategoryText { get; set; } = [];

    public List<EntityMMissionTerm> EntityMMissionTerm { get; set; } = [];

    public List<EntityMMissionUnlockCondition> EntityMMissionUnlockCondition { get; set; } = [];

    public List<EntityMMomBanner> EntityMMomBanner { get; set; } = [];

    public List<EntityMMomPointBanner> EntityMMomPointBanner { get; set; } = [];

    public List<EntityMMovie> EntityMMovie { get; set; } = [];

    public List<EntityMNaviCutIn> EntityMNaviCutIn { get; set; } = [];

    public List<EntityMNaviCutInContentGroup> EntityMNaviCutInContentGroup { get; set; } = [];

    public List<EntityMNaviCutInText> EntityMNaviCutInText { get; set; } = [];

    public List<EntityMNumericalFunction> EntityMNumericalFunction { get; set; } = [];

    public List<EntityMNumericalFunctionParameterGroup> EntityMNumericalFunctionParameterGroup { get; set; } = [];

    public List<EntityMNumericalParameterMap> EntityMNumericalParameterMap { get; set; } = [];

    public List<EntityMOmikuji> EntityMOmikuji { get; set; } = [];

    public List<EntityMOverrideHitEffectConditionCritical> EntityMOverrideHitEffectConditionCritical { get; set; } = [];

    public List<EntityMOverrideHitEffectConditionDamageAttribute> EntityMOverrideHitEffectConditionDamageAttribute { get; set; } = [];

    public List<EntityMOverrideHitEffectConditionGroup> EntityMOverrideHitEffectConditionGroup { get; set; } = [];

    public List<EntityMOverrideHitEffectConditionSkillExecutor> EntityMOverrideHitEffectConditionSkillExecutor { get; set; } = [];

    public List<EntityMParts> EntityMParts { get; set; } = [];

    public List<EntityMPartsEnhanced> EntityMPartsEnhanced { get; set; } = [];

    public List<EntityMPartsEnhancedSubStatus> EntityMPartsEnhancedSubStatus { get; set; } = [];

    public List<EntityMPartsGroup> EntityMPartsGroup { get; set; } = [];

    public List<EntityMPartsLevelUpPriceGroup> EntityMPartsLevelUpPriceGroup { get; set; } = [];

    public List<EntityMPartsLevelUpRateGroup> EntityMPartsLevelUpRateGroup { get; set; } = [];

    public List<EntityMPartsRarity> EntityMPartsRarity { get; set; } = [];

    public List<EntityMPartsSeries> EntityMPartsSeries { get; set; } = [];

    public List<EntityMPartsSeriesBonusAbilityGroup> EntityMPartsSeriesBonusAbilityGroup { get; set; } = [];

    public List<EntityMPartsStatusMain> EntityMPartsStatusMain { get; set; } = [];

    public List<EntityMPlatformPayment> EntityMPlatformPayment { get; set; } = [];

    public List<EntityMPlatformPaymentPrice> EntityMPlatformPaymentPrice { get; set; } = [];

    public List<EntityMPortalCageAccessPointFunctionGroup> EntityMPortalCageAccessPointFunctionGroup { get; set; } = [];

    public List<EntityMPortalCageAccessPointFunctionGroupSchedule> EntityMPortalCageAccessPointFunctionGroupSchedule { get; set; } = [];

    public List<EntityMPortalCageCharacterGroup> EntityMPortalCageCharacterGroup { get; set; } = [];

    public List<EntityMPortalCageGate> EntityMPortalCageGate { get; set; } = [];

    public List<EntityMPortalCageScene> EntityMPortalCageScene { get; set; } = [];

    public List<EntityMPossessionAcquisitionRoute> EntityMPossessionAcquisitionRoute { get; set; } = [];

    public List<EntityMPowerCalculationConstantValue> EntityMPowerCalculationConstantValue { get; set; } = [];

    public List<EntityMPowerReferenceStatusGroup> EntityMPowerReferenceStatusGroup { get; set; } = [];

    public List<EntityMPremiumItem> EntityMPremiumItem { get; set; } = [];

    public List<EntityMPvpBackground> EntityMPvpBackground { get; set; } = [];

    public List<EntityMPvpGrade> EntityMPvpGrade { get; set; } = [];

    public List<EntityMPvpGradeGroup> EntityMPvpGradeGroup { get; set; } = [];

    public List<EntityMPvpGradeOneMatchReward> EntityMPvpGradeOneMatchReward { get; set; } = [];

    public List<EntityMPvpGradeOneMatchRewardGroup> EntityMPvpGradeOneMatchRewardGroup { get; set; } = [];

    public List<EntityMPvpGradeWeeklyRewardGroup> EntityMPvpGradeWeeklyRewardGroup { get; set; } = [];

    public List<EntityMPvpReward> EntityMPvpReward { get; set; } = [];

    public List<EntityMPvpSeason> EntityMPvpSeason { get; set; } = [];

    public List<EntityMPvpSeasonGrade> EntityMPvpSeasonGrade { get; set; } = [];

    public List<EntityMPvpSeasonGrouping> EntityMPvpSeasonGrouping { get; set; } = [];

    public List<EntityMPvpSeasonRankReward> EntityMPvpSeasonRankReward { get; set; } = [];

    public List<EntityMPvpSeasonRankRewardGroup> EntityMPvpSeasonRankRewardGroup { get; set; } = [];

    public List<EntityMPvpSeasonRankRewardPerSeason> EntityMPvpSeasonRankRewardPerSeason { get; set; } = [];

    public List<EntityMPvpSeasonRankRewardRankGroup> EntityMPvpSeasonRankRewardRankGroup { get; set; } = [];

    public List<EntityMPvpWeeklyRankRewardGroup> EntityMPvpWeeklyRankRewardGroup { get; set; } = [];

    public List<EntityMPvpWeeklyRankRewardRankGroup> EntityMPvpWeeklyRankRewardRankGroup { get; set; } = [];

    public List<EntityMPvpWinStreakCountEffect> EntityMPvpWinStreakCountEffect { get; set; } = [];

    public List<EntityMQuest> EntityMQuest { get; set; } = [];

    public List<EntityMQuestBonus> EntityMQuestBonus { get; set; } = [];

    public List<EntityMQuestBonusAbility> EntityMQuestBonusAbility { get; set; } = [];

    public List<EntityMQuestBonusAllyCharacter> EntityMQuestBonusAllyCharacter { get; set; } = [];

    public List<EntityMQuestBonusCharacterGroup> EntityMQuestBonusCharacterGroup { get; set; } = [];

    public List<EntityMQuestBonusCostumeGroup> EntityMQuestBonusCostumeGroup { get; set; } = [];

    public List<EntityMQuestBonusCostumeSettingGroup> EntityMQuestBonusCostumeSettingGroup { get; set; } = [];

    public List<EntityMQuestBonusDropReward> EntityMQuestBonusDropReward { get; set; } = [];

    public List<EntityMQuestBonusEffectGroup> EntityMQuestBonusEffectGroup { get; set; } = [];

    public List<EntityMQuestBonusExp> EntityMQuestBonusExp { get; set; } = [];

    public List<EntityMQuestBonusTermGroup> EntityMQuestBonusTermGroup { get; set; } = [];

    public List<EntityMQuestBonusWeaponGroup> EntityMQuestBonusWeaponGroup { get; set; } = [];

    public List<EntityMQuestCampaign> EntityMQuestCampaign { get; set; } = [];

    public List<EntityMQuestCampaignEffectGroup> EntityMQuestCampaignEffectGroup { get; set; } = [];

    public List<EntityMQuestCampaignTargetGroup> EntityMQuestCampaignTargetGroup { get; set; } = [];

    public List<EntityMQuestCampaignTargetItemGroup> EntityMQuestCampaignTargetItemGroup { get; set; } = [];

    public List<EntityMQuestDeckMultiRestrictionGroup> EntityMQuestDeckMultiRestrictionGroup { get; set; } = [];

    public List<EntityMQuestDeckRestrictionGroup> EntityMQuestDeckRestrictionGroup { get; set; } = [];

    public List<EntityMQuestDeckRestrictionGroupUnlock> EntityMQuestDeckRestrictionGroupUnlock { get; set; } = [];

    public List<EntityMQuestDisplayAttributeGroup> EntityMQuestDisplayAttributeGroup { get; set; } = [];

    public List<EntityMQuestDisplayEnemyThumbnailReplace> EntityMQuestDisplayEnemyThumbnailReplace { get; set; } = [];

    public List<EntityMQuestFirstClearRewardGroup> EntityMQuestFirstClearRewardGroup { get; set; } = [];

    public List<EntityMQuestFirstClearRewardSwitch> EntityMQuestFirstClearRewardSwitch { get; set; } = [];

    public List<EntityMQuestMission> EntityMQuestMission { get; set; } = [];

    public List<EntityMQuestMissionConditionValueGroup> EntityMQuestMissionConditionValueGroup { get; set; } = [];

    public List<EntityMQuestMissionGroup> EntityMQuestMissionGroup { get; set; } = [];

    public List<EntityMQuestMissionReward> EntityMQuestMissionReward { get; set; } = [];

    public List<EntityMQuestPickupRewardGroup> EntityMQuestPickupRewardGroup { get; set; } = [];

    public List<EntityMQuestRelationMainFlow> EntityMQuestRelationMainFlow { get; set; } = [];

    public List<EntityMQuestReleaseConditionBigHuntScore> EntityMQuestReleaseConditionBigHuntScore { get; set; } = [];

    public List<EntityMQuestReleaseConditionCharacterLevel> EntityMQuestReleaseConditionCharacterLevel { get; set; } = [];

    public List<EntityMQuestReleaseConditionDeckPower> EntityMQuestReleaseConditionDeckPower { get; set; } = [];

    public List<EntityMQuestReleaseConditionGroup> EntityMQuestReleaseConditionGroup { get; set; } = [];

    public List<EntityMQuestReleaseConditionList> EntityMQuestReleaseConditionList { get; set; } = [];

    public List<EntityMQuestReleaseConditionQuestChallenge> EntityMQuestReleaseConditionQuestChallenge { get; set; } = [];

    public List<EntityMQuestReleaseConditionQuestClear> EntityMQuestReleaseConditionQuestClear { get; set; } = [];

    public List<EntityMQuestReleaseConditionUserLevel> EntityMQuestReleaseConditionUserLevel { get; set; } = [];

    public List<EntityMQuestReleaseConditionWeaponAcquisition> EntityMQuestReleaseConditionWeaponAcquisition { get; set; } = [];

    public List<EntityMQuestReplayFlowRewardGroup> EntityMQuestReplayFlowRewardGroup { get; set; } = [];

    public List<EntityMQuestScene> EntityMQuestScene { get; set; } = [];

    public List<EntityMQuestSceneBattle> EntityMQuestSceneBattle { get; set; } = [];

    public List<EntityMQuestSceneChoice> EntityMQuestSceneChoice { get; set; } = [];

    public List<EntityMQuestSceneChoiceCostumeEffectGroup> EntityMQuestSceneChoiceCostumeEffectGroup { get; set; } = [];

    public List<EntityMQuestSceneChoiceEffect> EntityMQuestSceneChoiceEffect { get; set; } = [];

    public List<EntityMQuestSceneChoiceWeaponEffectGroup> EntityMQuestSceneChoiceWeaponEffectGroup { get; set; } = [];

    public List<EntityMQuestSceneNotConfirmTitleDialog> EntityMQuestSceneNotConfirmTitleDialog { get; set; } = [];

    public List<EntityMQuestSceneOutgameBlendshapeMotion> EntityMQuestSceneOutgameBlendshapeMotion { get; set; } = [];

    public List<EntityMQuestScenePictureBookReplace> EntityMQuestScenePictureBookReplace { get; set; } = [];

    public List<EntityMQuestSchedule> EntityMQuestSchedule { get; set; } = [];

    public List<EntityMQuestScheduleCorrespondence> EntityMQuestScheduleCorrespondence { get; set; } = [];

    public List<EntityMReport> EntityMReport { get; set; } = [];

    public List<EntityMShop> EntityMShop { get; set; } = [];

    public List<EntityMShopDisplayPrice> EntityMShopDisplayPrice { get; set; } = [];

    public List<EntityMShopItem> EntityMShopItem { get; set; } = [];

    public List<EntityMShopItemAdditionalContent> EntityMShopItemAdditionalContent { get; set; } = [];

    public List<EntityMShopItemCell> EntityMShopItemCell { get; set; } = [];

    public List<EntityMShopItemCellGroup> EntityMShopItemCellGroup { get; set; } = [];

    public List<EntityMShopItemCellLimitedOpen> EntityMShopItemCellLimitedOpen { get; set; } = [];

    public List<EntityMShopItemCellTerm> EntityMShopItemCellTerm { get; set; } = [];

    public List<EntityMShopItemContentEffect> EntityMShopItemContentEffect { get; set; } = [];

    public List<EntityMShopItemContentMission> EntityMShopItemContentMission { get; set; } = [];

    public List<EntityMShopItemContentPossession> EntityMShopItemContentPossession { get; set; } = [];

    public List<EntityMShopItemLimitedStock> EntityMShopItemLimitedStock { get; set; } = [];

    public List<EntityMShopItemUserLevelCondition> EntityMShopItemUserLevelCondition { get; set; } = [];

    public List<EntityMShopReplaceableGem> EntityMShopReplaceableGem { get; set; } = [];

    public List<EntityMSideStoryQuest> EntityMSideStoryQuest { get; set; } = [];

    public List<EntityMSideStoryQuestLimitContent> EntityMSideStoryQuestLimitContent { get; set; } = [];

    public List<EntityMSideStoryQuestScene> EntityMSideStoryQuestScene { get; set; } = [];

    public List<EntityMSkill> EntityMSkill { get; set; } = [];

    public List<EntityMSkillAbnormal> EntityMSkillAbnormal { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviour> EntityMSkillAbnormalBehaviour { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionAbnormalResistance> EntityMSkillAbnormalBehaviourActionAbnormalResistance { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionAttributeDamageCorrection> EntityMSkillAbnormalBehaviourActionAttributeDamageCorrection { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionBuffResistance> EntityMSkillAbnormalBehaviourActionBuffResistance { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionDamage> EntityMSkillAbnormalBehaviourActionDamage { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionDamageMultiply> EntityMSkillAbnormalBehaviourActionDamageMultiply { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways> EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionDefaultSkillLottery> EntityMSkillAbnormalBehaviourActionDefaultSkillLottery { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionHitRatioDown> EntityMSkillAbnormalBehaviourActionHitRatioDown { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionModifyHateValue> EntityMSkillAbnormalBehaviourActionModifyHateValue { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionOverrideEvasionValue> EntityMSkillAbnormalBehaviourActionOverrideEvasionValue { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionOverrideHitEffect> EntityMSkillAbnormalBehaviourActionOverrideHitEffect { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionRecovery> EntityMSkillAbnormalBehaviourActionRecovery { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourActionTurnRestriction> EntityMSkillAbnormalBehaviourActionTurnRestriction { get; set; } = [];

    public List<EntityMSkillAbnormalBehaviourGroup> EntityMSkillAbnormalBehaviourGroup { get; set; } = [];

    public List<EntityMSkillAbnormalDamageMultiplyDetailAbnormal> EntityMSkillAbnormalDamageMultiplyDetailAbnormal { get; set; } = [];

    public List<EntityMSkillAbnormalDamageMultiplyDetailBuffAttached> EntityMSkillAbnormalDamageMultiplyDetailBuffAttached { get; set; } = [];

    public List<EntityMSkillAbnormalDamageMultiplyDetailCritical> EntityMSkillAbnormalDamageMultiplyDetailCritical { get; set; } = [];

    public List<EntityMSkillAbnormalDamageMultiplyDetailHitIndex> EntityMSkillAbnormalDamageMultiplyDetailHitIndex { get; set; } = [];

    public List<EntityMSkillAbnormalDamageMultiplyDetailSkillfulWeapon> EntityMSkillAbnormalDamageMultiplyDetailSkillfulWeapon { get; set; } = [];

    public List<EntityMSkillAbnormalLifetime> EntityMSkillAbnormalLifetime { get; set; } = [];

    public List<EntityMSkillAbnormalLifetimeBehaviourActivateCount> EntityMSkillAbnormalLifetimeBehaviourActivateCount { get; set; } = [];

    public List<EntityMSkillAbnormalLifetimeBehaviourFrameCount> EntityMSkillAbnormalLifetimeBehaviourFrameCount { get; set; } = [];

    public List<EntityMSkillAbnormalLifetimeBehaviourGroup> EntityMSkillAbnormalLifetimeBehaviourGroup { get; set; } = [];

    public List<EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount> EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount { get; set; } = [];

    public List<EntityMSkillAbnormalLifetimeBehaviourTurnCount> EntityMSkillAbnormalLifetimeBehaviourTurnCount { get; set; } = [];

    public List<EntityMSkillBehaviour> EntityMSkillBehaviour { get; set; } = [];

    public List<EntityMSkillBehaviourActionAbnormal> EntityMSkillBehaviourActionAbnormal { get; set; } = [];

    public List<EntityMSkillBehaviourActionActiveSkillDamageCorrection> EntityMSkillBehaviourActionActiveSkillDamageCorrection { get; set; } = [];

    public List<EntityMSkillBehaviourActionAdvanceActiveSkillCooltime> EntityMSkillBehaviourActionAdvanceActiveSkillCooltime { get; set; } = [];

    public List<EntityMSkillBehaviourActionAdvanceActiveSkillCooltimeImmediate> EntityMSkillBehaviourActionAdvanceActiveSkillCooltimeImmediate { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttack> EntityMSkillBehaviourActionAttack { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackClampHp> EntityMSkillBehaviourActionAttackClampHp { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackCombo> EntityMSkillBehaviourActionAttackCombo { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackFixedDamage> EntityMSkillBehaviourActionAttackFixedDamage { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackHpRatio> EntityMSkillBehaviourActionAttackHpRatio { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackIgnoreVitality> EntityMSkillBehaviourActionAttackIgnoreVitality { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackMainWeaponAttribute> EntityMSkillBehaviourActionAttackMainWeaponAttribute { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackSkillfulMainWeaponType> EntityMSkillBehaviourActionAttackSkillfulMainWeaponType { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttackVitality> EntityMSkillBehaviourActionAttackVitality { get; set; } = [];

    public List<EntityMSkillBehaviourActionAttributeDamageCorrection> EntityMSkillBehaviourActionAttributeDamageCorrection { get; set; } = [];

    public List<EntityMSkillBehaviourActionBuff> EntityMSkillBehaviourActionBuff { get; set; } = [];

    public List<EntityMSkillBehaviourActionChangestep> EntityMSkillBehaviourActionChangestep { get; set; } = [];

    public List<EntityMSkillBehaviourActionDamageCorrectionHpRatio> EntityMSkillBehaviourActionDamageCorrectionHpRatio { get; set; } = [];

    public List<EntityMSkillBehaviourActionDamageMultiply> EntityMSkillBehaviourActionDamageMultiply { get; set; } = [];

    public List<EntityMSkillBehaviourActionDefaultSkillLottery> EntityMSkillBehaviourActionDefaultSkillLottery { get; set; } = [];

    public List<EntityMSkillBehaviourActionExtendBuffCooltime> EntityMSkillBehaviourActionExtendBuffCooltime { get; set; } = [];

    public List<EntityMSkillBehaviourActionHpRatioDamage> EntityMSkillBehaviourActionHpRatioDamage { get; set; } = [];

    public List<EntityMSkillBehaviourActionOverlimitDamageMultiply> EntityMSkillBehaviourActionOverlimitDamageMultiply { get; set; } = [];

    public List<EntityMSkillBehaviourActionRecovery> EntityMSkillBehaviourActionRecovery { get; set; } = [];

    public List<EntityMSkillBehaviourActionRecoveryPointCorrection> EntityMSkillBehaviourActionRecoveryPointCorrection { get; set; } = [];

    public List<EntityMSkillBehaviourActionRemoveAbnormal> EntityMSkillBehaviourActionRemoveAbnormal { get; set; } = [];

    public List<EntityMSkillBehaviourActionRemoveBuff> EntityMSkillBehaviourActionRemoveBuff { get; set; } = [];

    public List<EntityMSkillBehaviourActionShortenActiveSkillCooltime> EntityMSkillBehaviourActionShortenActiveSkillCooltime { get; set; } = [];

    public List<EntityMSkillBehaviourActionSkillRecoveryPowerCorrection> EntityMSkillBehaviourActionSkillRecoveryPowerCorrection { get; set; } = [];

    public List<EntityMSkillBehaviourActivationConditionActivationUpperCount> EntityMSkillBehaviourActivationConditionActivationUpperCount { get; set; } = [];

    public List<EntityMSkillBehaviourActivationConditionAttribute> EntityMSkillBehaviourActivationConditionAttribute { get; set; } = [];

    public List<EntityMSkillBehaviourActivationConditionGroup> EntityMSkillBehaviourActivationConditionGroup { get; set; } = [];

    public List<EntityMSkillBehaviourActivationConditionHpRatio> EntityMSkillBehaviourActivationConditionHpRatio { get; set; } = [];

    public List<EntityMSkillBehaviourActivationConditionInSkillFlow> EntityMSkillBehaviourActivationConditionInSkillFlow { get; set; } = [];

    public List<EntityMSkillBehaviourActivationConditionWaveNumber> EntityMSkillBehaviourActivationConditionWaveNumber { get; set; } = [];

    public List<EntityMSkillBehaviourActivationMethod> EntityMSkillBehaviourActivationMethod { get; set; } = [];

    public List<EntityMSkillBehaviourGroup> EntityMSkillBehaviourGroup { get; set; } = [];

    public List<EntityMSkillBuff> EntityMSkillBuff { get; set; } = [];

    public List<EntityMSkillCasttime> EntityMSkillCasttime { get; set; } = [];

    public List<EntityMSkillCasttimeBehaviour> EntityMSkillCasttimeBehaviour { get; set; } = [];

    public List<EntityMSkillCasttimeBehaviourActionOnFrameUpdate> EntityMSkillCasttimeBehaviourActionOnFrameUpdate { get; set; } = [];

    public List<EntityMSkillCasttimeBehaviourActionOnSkillDamageCondition> EntityMSkillCasttimeBehaviourActionOnSkillDamageCondition { get; set; } = [];

    public List<EntityMSkillCasttimeBehaviourGroup> EntityMSkillCasttimeBehaviourGroup { get; set; } = [];

    public List<EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup> EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup { get; set; } = [];

    public List<EntityMSkillCooltimeBehaviour> EntityMSkillCooltimeBehaviour { get; set; } = [];

    public List<EntityMSkillCooltimeBehaviourGroup> EntityMSkillCooltimeBehaviourGroup { get; set; } = [];

    public List<EntityMSkillCooltimeBehaviourOnExecuteActiveSkill> EntityMSkillCooltimeBehaviourOnExecuteActiveSkill { get; set; } = [];

    public List<EntityMSkillCooltimeBehaviourOnExecuteCompanionSkill> EntityMSkillCooltimeBehaviourOnExecuteCompanionSkill { get; set; } = [];

    public List<EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill> EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill { get; set; } = [];

    public List<EntityMSkillCooltimeBehaviourOnFrameUpdate> EntityMSkillCooltimeBehaviourOnFrameUpdate { get; set; } = [];

    public List<EntityMSkillCooltimeBehaviourOnSkillDamage> EntityMSkillCooltimeBehaviourOnSkillDamage { get; set; } = [];

    public List<EntityMSkillDamageMultiplyAbnormalAttachedValueGroup> EntityMSkillDamageMultiplyAbnormalAttachedValueGroup { get; set; } = [];

    public List<EntityMSkillDamageMultiplyDetailAbnormalAttached> EntityMSkillDamageMultiplyDetailAbnormalAttached { get; set; } = [];

    public List<EntityMSkillDamageMultiplyDetailAlways> EntityMSkillDamageMultiplyDetailAlways { get; set; } = [];

    public List<EntityMSkillDamageMultiplyDetailBuffAttached> EntityMSkillDamageMultiplyDetailBuffAttached { get; set; } = [];

    public List<EntityMSkillDamageMultiplyDetailCritical> EntityMSkillDamageMultiplyDetailCritical { get; set; } = [];

    public List<EntityMSkillDamageMultiplyDetailHitIndex> EntityMSkillDamageMultiplyDetailHitIndex { get; set; } = [];

    public List<EntityMSkillDamageMultiplyDetailSkillfulWeaponType> EntityMSkillDamageMultiplyDetailSkillfulWeaponType { get; set; } = [];

    public List<EntityMSkillDamageMultiplyDetailSpecifiedCostumeType> EntityMSkillDamageMultiplyDetailSpecifiedCostumeType { get; set; } = [];

    public List<EntityMSkillDamageMultiplyHitIndexValueGroup> EntityMSkillDamageMultiplyHitIndexValueGroup { get; set; } = [];

    public List<EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup> EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup { get; set; } = [];

    public List<EntityMSkillDetail> EntityMSkillDetail { get; set; } = [];

    public List<EntityMSkillLevelGroup> EntityMSkillLevelGroup { get; set; } = [];

    public List<EntityMSkillRemoveAbnormalTargetAbnormalGroup> EntityMSkillRemoveAbnormalTargetAbnormalGroup { get; set; } = [];

    public List<EntityMSkillRemoveBuffFilterStatusKind> EntityMSkillRemoveBuffFilterStatusKind { get; set; } = [];

    public List<EntityMSkillReserveUiType> EntityMSkillReserveUiType { get; set; } = [];

    public List<EntityMSmartphoneChatGroup> EntityMSmartphoneChatGroup { get; set; } = [];

    public List<EntityMSmartphoneChatGroupMessage> EntityMSmartphoneChatGroupMessage { get; set; } = [];

    public List<EntityMSpeaker> EntityMSpeaker { get; set; } = [];

    public List<EntityMStainedGlass> EntityMStainedGlass { get; set; } = [];

    public List<EntityMStainedGlassStatusUpGroup> EntityMStainedGlassStatusUpGroup { get; set; } = [];

    public List<EntityMStainedGlassStatusUpTargetGroup> EntityMStainedGlassStatusUpTargetGroup { get; set; } = [];

    public List<EntityMThought> EntityMThought { get; set; } = [];

    public List<EntityMTip> EntityMTip { get; set; } = [];

    public List<EntityMTipBackgroundAsset> EntityMTipBackgroundAsset { get; set; } = [];

    public List<EntityMTipDisplayConditionGroup> EntityMTipDisplayConditionGroup { get; set; } = [];

    public List<EntityMTipGroup> EntityMTipGroup { get; set; } = [];

    public List<EntityMTipGroupBackgroundAsset> EntityMTipGroupBackgroundAsset { get; set; } = [];

    public List<EntityMTipGroupBackgroundAssetRelation> EntityMTipGroupBackgroundAssetRelation { get; set; } = [];

    public List<EntityMTipGroupSelection> EntityMTipGroupSelection { get; set; } = [];

    public List<EntityMTipGroupSituation> EntityMTipGroupSituation { get; set; } = [];

    public List<EntityMTipGroupSituationSeason> EntityMTipGroupSituationSeason { get; set; } = [];

    public List<EntityMTitleFlowMovie> EntityMTitleFlowMovie { get; set; } = [];

    public List<EntityMTitleStill> EntityMTitleStill { get; set; } = [];

    public List<EntityMTitleStillGroup> EntityMTitleStillGroup { get; set; } = [];

    public List<EntityMTutorialConsumePossessionGroup> EntityMTutorialConsumePossessionGroup { get; set; } = [];

    public List<EntityMTutorialDialog> EntityMTutorialDialog { get; set; } = [];

    public List<EntityMTutorialUnlockCondition> EntityMTutorialUnlockCondition { get; set; } = [];

    public List<EntityMUserLevel> EntityMUserLevel { get; set; } = [];

    public List<EntityMUserQuestSceneGrantPossession> EntityMUserQuestSceneGrantPossession { get; set; } = [];

    public List<EntityMWeapon> EntityMWeapon { get; set; } = [];

    public List<EntityMWeaponAbilityEnhancementMaterial> EntityMWeaponAbilityEnhancementMaterial { get; set; } = [];

    public List<EntityMWeaponAbilityGroup> EntityMWeaponAbilityGroup { get; set; } = [];

    public List<EntityMWeaponAwaken> EntityMWeaponAwaken { get; set; } = [];

    public List<EntityMWeaponAwakenAbility> EntityMWeaponAwakenAbility { get; set; } = [];

    public List<EntityMWeaponAwakenEffectGroup> EntityMWeaponAwakenEffectGroup { get; set; } = [];

    public List<EntityMWeaponAwakenMaterialGroup> EntityMWeaponAwakenMaterialGroup { get; set; } = [];

    public List<EntityMWeaponAwakenStatusUpGroup> EntityMWeaponAwakenStatusUpGroup { get; set; } = [];

    public List<EntityMWeaponBaseStatus> EntityMWeaponBaseStatus { get; set; } = [];

    public List<EntityMWeaponConsumeExchangeConsumableItemGroup> EntityMWeaponConsumeExchangeConsumableItemGroup { get; set; } = [];

    public List<EntityMWeaponEnhanced> EntityMWeaponEnhanced { get; set; } = [];

    public List<EntityMWeaponEnhancedAbility> EntityMWeaponEnhancedAbility { get; set; } = [];

    public List<EntityMWeaponEnhancedSkill> EntityMWeaponEnhancedSkill { get; set; } = [];

    public List<EntityMWeaponEvolutionGroup> EntityMWeaponEvolutionGroup { get; set; } = [];

    public List<EntityMWeaponEvolutionMaterialGroup> EntityMWeaponEvolutionMaterialGroup { get; set; } = [];

    public List<EntityMWeaponFieldEffectDecreasePoint> EntityMWeaponFieldEffectDecreasePoint { get; set; } = [];

    public List<EntityMWeaponRarity> EntityMWeaponRarity { get; set; } = [];

    public List<EntityMWeaponRarityLimitBreakMaterialGroup> EntityMWeaponRarityLimitBreakMaterialGroup { get; set; } = [];

    public List<EntityMWeaponSkillEnhancementMaterial> EntityMWeaponSkillEnhancementMaterial { get; set; } = [];

    public List<EntityMWeaponSkillGroup> EntityMWeaponSkillGroup { get; set; } = [];

    public List<EntityMWeaponSpecificEnhance> EntityMWeaponSpecificEnhance { get; set; } = [];

    public List<EntityMWeaponSpecificLimitBreakMaterialGroup> EntityMWeaponSpecificLimitBreakMaterialGroup { get; set; } = [];

    public List<EntityMWeaponStatusCalculation> EntityMWeaponStatusCalculation { get; set; } = [];

    public List<EntityMWeaponStoryReleaseConditionGroup> EntityMWeaponStoryReleaseConditionGroup { get; set; } = [];

    public List<EntityMWeaponStoryReleaseConditionOperation> EntityMWeaponStoryReleaseConditionOperation { get; set; } = [];

    public List<EntityMWeaponStoryReleaseConditionOperationGroup> EntityMWeaponStoryReleaseConditionOperationGroup { get; set; } = [];

    public List<EntityMWebviewMission> EntityMWebviewMission { get; set; } = [];

    public List<EntityMWebviewMissionTitleText> EntityMWebviewMissionTitleText { get; set; } = [];

    public List<EntityMWebviewPanelMission> EntityMWebviewPanelMission { get; set; } = [];

    public List<EntityMWebviewPanelMissionCompleteFlavorText> EntityMWebviewPanelMissionCompleteFlavorText { get; set; } = [];

    public List<EntityMWebviewPanelMissionPage> EntityMWebviewPanelMissionPage { get; set; } = [];

}
