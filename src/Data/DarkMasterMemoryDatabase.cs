using MariesWonderland.Models.Tables;

namespace NierReincarnation.Core.Dark;

public sealed class DarkMasterMemoryDatabase
{
    public EntityMAbilityTable EntityMAbilityTable { get; }

    public EntityMAbilityBehaviourTable EntityMAbilityBehaviourTable { get; }

    public EntityMAbilityBehaviourActionBlessTable EntityMAbilityBehaviourActionBlessTable { get; }

    public EntityMAbilityBehaviourActionPassiveSkillTable EntityMAbilityBehaviourActionPassiveSkillTable { get; }

    public EntityMAbilityBehaviourActionStatusTable EntityMAbilityBehaviourActionStatusTable { get; }

    public EntityMAbilityBehaviourActionStatusDownTable EntityMAbilityBehaviourActionStatusDownTable { get; }

    public EntityMAbilityBehaviourGroupTable EntityMAbilityBehaviourGroupTable { get; }

    public EntityMAbilityDetailTable EntityMAbilityDetailTable { get; }

    public EntityMAbilityLevelGroupTable EntityMAbilityLevelGroupTable { get; }

    public EntityMAbilityStatusTable EntityMAbilityStatusTable { get; }

    public EntityMActorTable EntityMActorTable { get; }

    public EntityMActorAnimationTable EntityMActorAnimationTable { get; }

    public EntityMActorAnimationCategoryTable EntityMActorAnimationCategoryTable { get; }

    public EntityMActorAnimationControllerTable EntityMActorAnimationControllerTable { get; }

    public EntityMActorObjectTable EntityMActorObjectTable { get; }

    public EntityMAppealDialogTable EntityMAppealDialogTable { get; }

    public EntityMAssetBackgroundTable EntityMAssetBackgroundTable { get; }

    public EntityMAssetCalculatorTable EntityMAssetCalculatorTable { get; }

    public EntityMAssetDataSettingTable EntityMAssetDataSettingTable { get; }

    public EntityMAssetEffectTable EntityMAssetEffectTable { get; }

    public EntityMAssetGradeIconTable EntityMAssetGradeIconTable { get; }

    public EntityMAssetTimelineTable EntityMAssetTimelineTable { get; }

    public EntityMAssetTurnbattlePrefabTable EntityMAssetTurnbattlePrefabTable { get; }

    public EntityMBattleTable EntityMBattleTable { get; }

    public EntityMBattleActorAiTable EntityMBattleActorAiTable { get; }

    public EntityMBattleActorSkillAiGroupTable EntityMBattleActorSkillAiGroupTable { get; }

    public EntityMBattleAdditionalAbilityTable EntityMBattleAdditionalAbilityTable { get; }

    public EntityMBattleAttributeDamageCoefficientDefineTable EntityMBattleAttributeDamageCoefficientDefineTable { get; }

    public EntityMBattleAttributeDamageCoefficientGroupTable EntityMBattleAttributeDamageCoefficientGroupTable { get; }

    public EntityMBattleBgmSetTable EntityMBattleBgmSetTable { get; }

    public EntityMBattleBgmSetGroupTable EntityMBattleBgmSetGroupTable { get; }

    public EntityMBattleBigHuntTable EntityMBattleBigHuntTable { get; }

    public EntityMBattleBigHuntDamageThresholdGroupTable EntityMBattleBigHuntDamageThresholdGroupTable { get; }

    public EntityMBattleBigHuntKnockDownGaugeValueConfigGroupTable EntityMBattleBigHuntKnockDownGaugeValueConfigGroupTable { get; }

    public EntityMBattleBigHuntPhaseGroupTable EntityMBattleBigHuntPhaseGroupTable { get; }

    public EntityMBattleCompanionSkillAiGroupTable EntityMBattleCompanionSkillAiGroupTable { get; }

    public EntityMBattleCostumeSkillFireActTable EntityMBattleCostumeSkillFireActTable { get; }

    public EntityMBattleCostumeSkillSeTable EntityMBattleCostumeSkillSeTable { get; }

    public EntityMBattleDropRewardTable EntityMBattleDropRewardTable { get; }

    public EntityMBattleEnemySizeTypeConfigTable EntityMBattleEnemySizeTypeConfigTable { get; }

    public EntityMBattleEventTable EntityMBattleEventTable { get; }

    public EntityMBattleEventGroupTable EntityMBattleEventGroupTable { get; }

    public EntityMBattleEventReceiverBehaviourGroupTable EntityMBattleEventReceiverBehaviourGroupTable { get; }

    public EntityMBattleEventReceiverBehaviourHudActSequenceTable EntityMBattleEventReceiverBehaviourHudActSequenceTable { get; }

    public EntityMBattleEventReceiverBehaviourRadioMessageTable EntityMBattleEventReceiverBehaviourRadioMessageTable { get; }

    public EntityMBattleEventTriggerBehaviourBattleStartTable EntityMBattleEventTriggerBehaviourBattleStartTable { get; }

    public EntityMBattleEventTriggerBehaviourGroupTable EntityMBattleEventTriggerBehaviourGroupTable { get; }

    public EntityMBattleEventTriggerBehaviourWaveStartTable EntityMBattleEventTriggerBehaviourWaveStartTable { get; }

    public EntityMBattleGeneralViewConfigurationTable EntityMBattleGeneralViewConfigurationTable { get; }

    public EntityMBattleGroupTable EntityMBattleGroupTable { get; }

    public EntityMBattleNpcTable EntityMBattleNpcTable { get; }

    public EntityMBattleNpcCharacterTable EntityMBattleNpcCharacterTable { get; }

    public EntityMBattleNpcCharacterBoardTable EntityMBattleNpcCharacterBoardTable { get; }

    public EntityMBattleNpcCharacterBoardAbilityTable EntityMBattleNpcCharacterBoardAbilityTable { get; }

    public EntityMBattleNpcCharacterBoardCompleteRewardTable EntityMBattleNpcCharacterBoardCompleteRewardTable { get; }

    public EntityMBattleNpcCharacterBoardStatusUpTable EntityMBattleNpcCharacterBoardStatusUpTable { get; }

    public EntityMBattleNpcCharacterCostumeLevelBonusTable EntityMBattleNpcCharacterCostumeLevelBonusTable { get; }

    public EntityMBattleNpcCharacterRebirthTable EntityMBattleNpcCharacterRebirthTable { get; }

    public EntityMBattleNpcCharacterViewerFieldTable EntityMBattleNpcCharacterViewerFieldTable { get; }

    public EntityMBattleNpcCompanionTable EntityMBattleNpcCompanionTable { get; }

    public EntityMBattleNpcCostumeTable EntityMBattleNpcCostumeTable { get; }

    public EntityMBattleNpcCostumeActiveSkillTable EntityMBattleNpcCostumeActiveSkillTable { get; }

    public EntityMBattleNpcCostumeAwakenStatusUpTable EntityMBattleNpcCostumeAwakenStatusUpTable { get; }

    public EntityMBattleNpcCostumeLevelBonusReevaluateTable EntityMBattleNpcCostumeLevelBonusReevaluateTable { get; }

    public EntityMBattleNpcCostumeLevelBonusReleaseStatusTable EntityMBattleNpcCostumeLevelBonusReleaseStatusTable { get; }

    public EntityMBattleNpcCostumeLotteryEffectTable EntityMBattleNpcCostumeLotteryEffectTable { get; }

    public EntityMBattleNpcCostumeLotteryEffectAbilityTable EntityMBattleNpcCostumeLotteryEffectAbilityTable { get; }

    public EntityMBattleNpcCostumeLotteryEffectPendingTable EntityMBattleNpcCostumeLotteryEffectPendingTable { get; }

    public EntityMBattleNpcCostumeLotteryEffectStatusUpTable EntityMBattleNpcCostumeLotteryEffectStatusUpTable { get; }

    public EntityMBattleNpcDeckTable EntityMBattleNpcDeckTable { get; }

    public EntityMBattleNpcDeckBackupTable EntityMBattleNpcDeckBackupTable { get; }

    public EntityMBattleNpcDeckCharacterTable EntityMBattleNpcDeckCharacterTable { get; }

    public EntityMBattleNpcDeckCharacterDressupCostumeTable EntityMBattleNpcDeckCharacterDressupCostumeTable { get; }

    public EntityMBattleNpcDeckCharacterDropCategoryTable EntityMBattleNpcDeckCharacterDropCategoryTable { get; }

    public EntityMBattleNpcDeckCharacterTypeTable EntityMBattleNpcDeckCharacterTypeTable { get; }

    public EntityMBattleNpcDeckLimitContentBackupTable EntityMBattleNpcDeckLimitContentBackupTable { get; }

    public EntityMBattleNpcDeckLimitContentBackupRestoredTable EntityMBattleNpcDeckLimitContentBackupRestoredTable { get; }

    public EntityMBattleNpcDeckLimitContentDeletedCharacterTable EntityMBattleNpcDeckLimitContentDeletedCharacterTable { get; }

    public EntityMBattleNpcDeckLimitContentRestrictedTable EntityMBattleNpcDeckLimitContentRestrictedTable { get; }

    public EntityMBattleNpcDeckPartsGroupTable EntityMBattleNpcDeckPartsGroupTable { get; }

    public EntityMBattleNpcDeckSubWeaponGroupTable EntityMBattleNpcDeckSubWeaponGroupTable { get; }

    public EntityMBattleNpcDeckTypeNoteTable EntityMBattleNpcDeckTypeNoteTable { get; }

    public EntityMBattleNpcPartsTable EntityMBattleNpcPartsTable { get; }

    public EntityMBattleNpcPartsGroupNoteTable EntityMBattleNpcPartsGroupNoteTable { get; }

    public EntityMBattleNpcPartsPresetTable EntityMBattleNpcPartsPresetTable { get; }

    public EntityMBattleNpcPartsPresetTagTable EntityMBattleNpcPartsPresetTagTable { get; }

    public EntityMBattleNpcPartsStatusSubTable EntityMBattleNpcPartsStatusSubTable { get; }

    public EntityMBattleNpcSpecialEndActTable EntityMBattleNpcSpecialEndActTable { get; }

    public EntityMBattleNpcWeaponTable EntityMBattleNpcWeaponTable { get; }

    public EntityMBattleNpcWeaponAbilityTable EntityMBattleNpcWeaponAbilityTable { get; }

    public EntityMBattleNpcWeaponAbilityReevaluateTable EntityMBattleNpcWeaponAbilityReevaluateTable { get; }

    public EntityMBattleNpcWeaponAwakenTable EntityMBattleNpcWeaponAwakenTable { get; }

    public EntityMBattleNpcWeaponNoteTable EntityMBattleNpcWeaponNoteTable { get; }

    public EntityMBattleNpcWeaponNoteReevaluateTable EntityMBattleNpcWeaponNoteReevaluateTable { get; }

    public EntityMBattleNpcWeaponSkillTable EntityMBattleNpcWeaponSkillTable { get; }

    public EntityMBattleNpcWeaponStoryTable EntityMBattleNpcWeaponStoryTable { get; }

    public EntityMBattleNpcWeaponStoryReevaluateTable EntityMBattleNpcWeaponStoryReevaluateTable { get; }

    public EntityMBattleProgressUiTypeTable EntityMBattleProgressUiTypeTable { get; }

    public EntityMBattleQuestSceneBgmTable EntityMBattleQuestSceneBgmTable { get; }

    public EntityMBattleQuestSceneBgmSetGroupTable EntityMBattleQuestSceneBgmSetGroupTable { get; }

    public EntityMBattleRentalDeckTable EntityMBattleRentalDeckTable { get; }

    public EntityMBattleSkillBehaviourHitDamageConfigurationTable EntityMBattleSkillBehaviourHitDamageConfigurationTable { get; }

    public EntityMBattleSkillFireActTable EntityMBattleSkillFireActTable { get; }

    public EntityMBattleSkillFireActConditionAttributeTypeTable EntityMBattleSkillFireActConditionAttributeTypeTable { get; }

    public EntityMBattleSkillFireActConditionGroupTable EntityMBattleSkillFireActConditionGroupTable { get; }

    public EntityMBattleSkillFireActConditionSkillCategoryTypeTable EntityMBattleSkillFireActConditionSkillCategoryTypeTable { get; }

    public EntityMBattleSkillFireActConditionWeaponTypeTable EntityMBattleSkillFireActConditionWeaponTypeTable { get; }

    public EntityMBeginnerCampaignTable EntityMBeginnerCampaignTable { get; }

    public EntityMBigHuntBossTable EntityMBigHuntBossTable { get; }

    public EntityMBigHuntBossGradeGroupTable EntityMBigHuntBossGradeGroupTable { get; }

    public EntityMBigHuntBossGradeGroupAttributeTable EntityMBigHuntBossGradeGroupAttributeTable { get; }

    public EntityMBigHuntBossQuestTable EntityMBigHuntBossQuestTable { get; }

    public EntityMBigHuntBossQuestGroupTable EntityMBigHuntBossQuestGroupTable { get; }

    public EntityMBigHuntBossQuestGroupChallengeCategoryTable EntityMBigHuntBossQuestGroupChallengeCategoryTable { get; }

    public EntityMBigHuntLinkTable EntityMBigHuntLinkTable { get; }

    public EntityMBigHuntQuestTable EntityMBigHuntQuestTable { get; }

    public EntityMBigHuntQuestGroupTable EntityMBigHuntQuestGroupTable { get; }

    public EntityMBigHuntQuestScoreCoefficientTable EntityMBigHuntQuestScoreCoefficientTable { get; }

    public EntityMBigHuntRewardGroupTable EntityMBigHuntRewardGroupTable { get; }

    public EntityMBigHuntScheduleTable EntityMBigHuntScheduleTable { get; }

    public EntityMBigHuntScoreRewardGroupTable EntityMBigHuntScoreRewardGroupTable { get; }

    public EntityMBigHuntScoreRewardGroupScheduleTable EntityMBigHuntScoreRewardGroupScheduleTable { get; }

    public EntityMBigHuntWeeklyAttributeScoreRewardGroupScheduleTable EntityMBigHuntWeeklyAttributeScoreRewardGroupScheduleTable { get; }

    public EntityMCageMemoryTable EntityMCageMemoryTable { get; }

    public EntityMCageOrnamentTable EntityMCageOrnamentTable { get; }

    public EntityMCageOrnamentMainQuestChapterStillTable EntityMCageOrnamentMainQuestChapterStillTable { get; }

    public EntityMCageOrnamentRewardTable EntityMCageOrnamentRewardTable { get; }

    public EntityMCageOrnamentStillReleaseConditionTable EntityMCageOrnamentStillReleaseConditionTable { get; }

    public EntityMCatalogCompanionTable EntityMCatalogCompanionTable { get; }

    public EntityMCatalogCostumeTable EntityMCatalogCostumeTable { get; }

    public EntityMCatalogPartsGroupTable EntityMCatalogPartsGroupTable { get; }

    public EntityMCatalogTermTable EntityMCatalogTermTable { get; }

    public EntityMCatalogThoughtTable EntityMCatalogThoughtTable { get; }

    public EntityMCatalogWeaponTable EntityMCatalogWeaponTable { get; }

    public EntityMCharacterTable EntityMCharacterTable { get; }

    public EntityMCharacterBoardTable EntityMCharacterBoardTable { get; }

    public EntityMCharacterBoardAbilityTable EntityMCharacterBoardAbilityTable { get; }

    public EntityMCharacterBoardAbilityMaxLevelTable EntityMCharacterBoardAbilityMaxLevelTable { get; }

    public EntityMCharacterBoardAssignmentTable EntityMCharacterBoardAssignmentTable { get; }

    public EntityMCharacterBoardCategoryTable EntityMCharacterBoardCategoryTable { get; }

    public EntityMCharacterBoardCompleteRewardTable EntityMCharacterBoardCompleteRewardTable { get; }

    public EntityMCharacterBoardCompleteRewardGroupTable EntityMCharacterBoardCompleteRewardGroupTable { get; }

    public EntityMCharacterBoardConditionTable EntityMCharacterBoardConditionTable { get; }

    public EntityMCharacterBoardConditionDetailTable EntityMCharacterBoardConditionDetailTable { get; }

    public EntityMCharacterBoardConditionGroupTable EntityMCharacterBoardConditionGroupTable { get; }

    public EntityMCharacterBoardConditionIgnoreTable EntityMCharacterBoardConditionIgnoreTable { get; }

    public EntityMCharacterBoardEffectTargetGroupTable EntityMCharacterBoardEffectTargetGroupTable { get; }

    public EntityMCharacterBoardGroupTable EntityMCharacterBoardGroupTable { get; }

    public EntityMCharacterBoardPanelTable EntityMCharacterBoardPanelTable { get; }

    public EntityMCharacterBoardPanelReleaseEffectGroupTable EntityMCharacterBoardPanelReleaseEffectGroupTable { get; }

    public EntityMCharacterBoardPanelReleasePossessionGroupTable EntityMCharacterBoardPanelReleasePossessionGroupTable { get; }

    public EntityMCharacterBoardPanelReleaseRewardGroupTable EntityMCharacterBoardPanelReleaseRewardGroupTable { get; }

    public EntityMCharacterBoardStatusUpTable EntityMCharacterBoardStatusUpTable { get; }

    public EntityMCharacterDisplaySwitchTable EntityMCharacterDisplaySwitchTable { get; }

    public EntityMCharacterLevelBonusAbilityGroupTable EntityMCharacterLevelBonusAbilityGroupTable { get; }

    public EntityMCharacterRebirthTable EntityMCharacterRebirthTable { get; }

    public EntityMCharacterRebirthMaterialGroupTable EntityMCharacterRebirthMaterialGroupTable { get; }

    public EntityMCharacterRebirthStepGroupTable EntityMCharacterRebirthStepGroupTable { get; }

    public EntityMCharacterViewerActorIconTable EntityMCharacterViewerActorIconTable { get; }

    public EntityMCharacterViewerFieldTable EntityMCharacterViewerFieldTable { get; }

    public EntityMCharacterViewerFieldSettingsTable EntityMCharacterViewerFieldSettingsTable { get; }

    public EntityMCharacterVoiceUnlockConditionTable EntityMCharacterVoiceUnlockConditionTable { get; }

    public EntityMCollectionBonusEffectTable EntityMCollectionBonusEffectTable { get; }

    public EntityMCollectionBonusQuestAssignmentTable EntityMCollectionBonusQuestAssignmentTable { get; }

    public EntityMCollectionBonusQuestAssignmentGroupTable EntityMCollectionBonusQuestAssignmentGroupTable { get; }

    public EntityMComboCalculationSettingTable EntityMComboCalculationSettingTable { get; }

    public EntityMComebackCampaignTable EntityMComebackCampaignTable { get; }

    public EntityMCompanionTable EntityMCompanionTable { get; }

    public EntityMCompanionAbilityGroupTable EntityMCompanionAbilityGroupTable { get; }

    public EntityMCompanionAbilityLevelTable EntityMCompanionAbilityLevelTable { get; }

    public EntityMCompanionBaseStatusTable EntityMCompanionBaseStatusTable { get; }

    public EntityMCompanionCategoryTable EntityMCompanionCategoryTable { get; }

    public EntityMCompanionDuplicationExchangePossessionGroupTable EntityMCompanionDuplicationExchangePossessionGroupTable { get; }

    public EntityMCompanionEnhancedTable EntityMCompanionEnhancedTable { get; }

    public EntityMCompanionEnhancementMaterialTable EntityMCompanionEnhancementMaterialTable { get; }

    public EntityMCompanionSkillLevelTable EntityMCompanionSkillLevelTable { get; }

    public EntityMCompanionStatusCalculationTable EntityMCompanionStatusCalculationTable { get; }

    public EntityMCompleteMissionGroupTable EntityMCompleteMissionGroupTable { get; }

    public EntityMConfigTable EntityMConfigTable { get; }

    public EntityMConsumableItemTable EntityMConsumableItemTable { get; }

    public EntityMConsumableItemEffectTable EntityMConsumableItemEffectTable { get; }

    public EntityMConsumableItemTermTable EntityMConsumableItemTermTable { get; }

    public EntityMContentsStoryTable EntityMContentsStoryTable { get; }

    public EntityMCostumeTable EntityMCostumeTable { get; }

    public EntityMCostumeAbilityGroupTable EntityMCostumeAbilityGroupTable { get; }

    public EntityMCostumeAbilityLevelGroupTable EntityMCostumeAbilityLevelGroupTable { get; }

    public EntityMCostumeActiveSkillEnhancementMaterialTable EntityMCostumeActiveSkillEnhancementMaterialTable { get; }

    public EntityMCostumeActiveSkillGroupTable EntityMCostumeActiveSkillGroupTable { get; }

    public EntityMCostumeAnimationStepTable EntityMCostumeAnimationStepTable { get; }

    public EntityMCostumeAutoOrganizationConditionTable EntityMCostumeAutoOrganizationConditionTable { get; }

    public EntityMCostumeAwakenTable EntityMCostumeAwakenTable { get; }

    public EntityMCostumeAwakenAbilityTable EntityMCostumeAwakenAbilityTable { get; }

    public EntityMCostumeAwakenEffectGroupTable EntityMCostumeAwakenEffectGroupTable { get; }

    public EntityMCostumeAwakenItemAcquireTable EntityMCostumeAwakenItemAcquireTable { get; }

    public EntityMCostumeAwakenMaterialGroupTable EntityMCostumeAwakenMaterialGroupTable { get; }

    public EntityMCostumeAwakenPriceGroupTable EntityMCostumeAwakenPriceGroupTable { get; }

    public EntityMCostumeAwakenStatusUpGroupTable EntityMCostumeAwakenStatusUpGroupTable { get; }

    public EntityMCostumeAwakenStepMaterialGroupTable EntityMCostumeAwakenStepMaterialGroupTable { get; }

    public EntityMCostumeBaseStatusTable EntityMCostumeBaseStatusTable { get; }

    public EntityMCostumeCollectionBonusTable EntityMCostumeCollectionBonusTable { get; }

    public EntityMCostumeCollectionBonusGroupTable EntityMCostumeCollectionBonusGroupTable { get; }

    public EntityMCostumeDefaultSkillGroupTable EntityMCostumeDefaultSkillGroupTable { get; }

    public EntityMCostumeDefaultSkillLotteryGroupTable EntityMCostumeDefaultSkillLotteryGroupTable { get; }

    public EntityMCostumeDeleteTable EntityMCostumeDeleteTable { get; }

    public EntityMCostumeDisplayCoordinateAdjustmentTable EntityMCostumeDisplayCoordinateAdjustmentTable { get; }

    public EntityMCostumeDisplaySwitchTable EntityMCostumeDisplaySwitchTable { get; }

    public EntityMCostumeDuplicationExchangePossessionGroupTable EntityMCostumeDuplicationExchangePossessionGroupTable { get; }

    public EntityMCostumeEmblemTable EntityMCostumeEmblemTable { get; }

    public EntityMCostumeEnhancedTable EntityMCostumeEnhancedTable { get; }

    public EntityMCostumeLevelBonusTable EntityMCostumeLevelBonusTable { get; }

    public EntityMCostumeLimitBreakMaterialGroupTable EntityMCostumeLimitBreakMaterialGroupTable { get; }

    public EntityMCostumeLimitBreakMaterialRarityGroupTable EntityMCostumeLimitBreakMaterialRarityGroupTable { get; }

    public EntityMCostumeLotteryEffectTable EntityMCostumeLotteryEffectTable { get; }

    public EntityMCostumeLotteryEffectMaterialGroupTable EntityMCostumeLotteryEffectMaterialGroupTable { get; }

    public EntityMCostumeLotteryEffectOddsGroupTable EntityMCostumeLotteryEffectOddsGroupTable { get; }

    public EntityMCostumeLotteryEffectReleaseScheduleTable EntityMCostumeLotteryEffectReleaseScheduleTable { get; }

    public EntityMCostumeLotteryEffectTargetAbilityTable EntityMCostumeLotteryEffectTargetAbilityTable { get; }

    public EntityMCostumeLotteryEffectTargetStatusUpTable EntityMCostumeLotteryEffectTargetStatusUpTable { get; }

    public EntityMCostumeOverflowExchangePossessionGroupTable EntityMCostumeOverflowExchangePossessionGroupTable { get; }

    public EntityMCostumeProperAttributeHpBonusTable EntityMCostumeProperAttributeHpBonusTable { get; }

    public EntityMCostumeRarityTable EntityMCostumeRarityTable { get; }

    public EntityMCostumeSpecialActActiveSkillTable EntityMCostumeSpecialActActiveSkillTable { get; }

    public EntityMCostumeSpecialActActiveSkillConditionAttributeTable EntityMCostumeSpecialActActiveSkillConditionAttributeTable { get; }

    public EntityMCostumeStatusCalculationTable EntityMCostumeStatusCalculationTable { get; }

    public EntityMDeckEntrustCoefficientAttributeTable EntityMDeckEntrustCoefficientAttributeTable { get; }

    public EntityMDeckEntrustCoefficientPartsSeriesBonusCountTable EntityMDeckEntrustCoefficientPartsSeriesBonusCountTable { get; }

    public EntityMDeckEntrustCoefficientStatusTable EntityMDeckEntrustCoefficientStatusTable { get; }

    public EntityMDokanTable EntityMDokanTable { get; }

    public EntityMDokanContentGroupTable EntityMDokanContentGroupTable { get; }

    public EntityMDokanTextTable EntityMDokanTextTable { get; }

    public EntityMEnhanceCampaignTable EntityMEnhanceCampaignTable { get; }

    public EntityMEnhanceCampaignTargetGroupTable EntityMEnhanceCampaignTargetGroupTable { get; }

    public EntityMEvaluateConditionTable EntityMEvaluateConditionTable { get; }

    public EntityMEvaluateConditionValueGroupTable EntityMEvaluateConditionValueGroupTable { get; }

    public EntityMEventQuestChapterTable EntityMEventQuestChapterTable { get; }

    public EntityMEventQuestChapterCharacterTable EntityMEventQuestChapterCharacterTable { get; }

    public EntityMEventQuestChapterDifficultyLimitContentUnlockTable EntityMEventQuestChapterDifficultyLimitContentUnlockTable { get; }

    public EntityMEventQuestChapterLimitContentRelationTable EntityMEventQuestChapterLimitContentRelationTable { get; }

    public EntityMEventQuestDailyGroupTable EntityMEventQuestDailyGroupTable { get; }

    public EntityMEventQuestDailyGroupCompleteRewardTable EntityMEventQuestDailyGroupCompleteRewardTable { get; }

    public EntityMEventQuestDailyGroupMessageTable EntityMEventQuestDailyGroupMessageTable { get; }

    public EntityMEventQuestDailyGroupTargetChapterTable EntityMEventQuestDailyGroupTargetChapterTable { get; }

    public EntityMEventQuestDisplayItemGroupTable EntityMEventQuestDisplayItemGroupTable { get; }

    public EntityMEventQuestGuerrillaFreeOpenTable EntityMEventQuestGuerrillaFreeOpenTable { get; }

    public EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondenceTable EntityMEventQuestGuerrillaFreeOpenScheduleCorrespondenceTable { get; }

    public EntityMEventQuestLabyrinthMobTable EntityMEventQuestLabyrinthMobTable { get; }

    public EntityMEventQuestLabyrinthQuestDisplayTable EntityMEventQuestLabyrinthQuestDisplayTable { get; }

    public EntityMEventQuestLabyrinthQuestEffectDescriptionAbilityTable EntityMEventQuestLabyrinthQuestEffectDescriptionAbilityTable { get; }

    public EntityMEventQuestLabyrinthQuestEffectDescriptionFreeTable EntityMEventQuestLabyrinthQuestEffectDescriptionFreeTable { get; }

    public EntityMEventQuestLabyrinthQuestEffectDisplayTable EntityMEventQuestLabyrinthQuestEffectDisplayTable { get; }

    public EntityMEventQuestLabyrinthRewardGroupTable EntityMEventQuestLabyrinthRewardGroupTable { get; }

    public EntityMEventQuestLabyrinthSeasonTable EntityMEventQuestLabyrinthSeasonTable { get; }

    public EntityMEventQuestLabyrinthSeasonRewardGroupTable EntityMEventQuestLabyrinthSeasonRewardGroupTable { get; }

    public EntityMEventQuestLabyrinthStageTable EntityMEventQuestLabyrinthStageTable { get; }

    public EntityMEventQuestLabyrinthStageAccumulationRewardGroupTable EntityMEventQuestLabyrinthStageAccumulationRewardGroupTable { get; }

    public EntityMEventQuestLimitContentTable EntityMEventQuestLimitContentTable { get; }

    public EntityMEventQuestLimitContentDeckRestrictionTable EntityMEventQuestLimitContentDeckRestrictionTable { get; }

    public EntityMEventQuestLimitContentDeckRestrictionTargetTable EntityMEventQuestLimitContentDeckRestrictionTargetTable { get; }

    public EntityMEventQuestLinkTable EntityMEventQuestLinkTable { get; }

    public EntityMEventQuestSequenceTable EntityMEventQuestSequenceTable { get; }

    public EntityMEventQuestSequenceGroupTable EntityMEventQuestSequenceGroupTable { get; }

    public EntityMEventQuestTowerAccumulationRewardTable EntityMEventQuestTowerAccumulationRewardTable { get; }

    public EntityMEventQuestTowerAccumulationRewardGroupTable EntityMEventQuestTowerAccumulationRewardGroupTable { get; }

    public EntityMEventQuestTowerAssetTable EntityMEventQuestTowerAssetTable { get; }

    public EntityMEventQuestTowerRewardGroupTable EntityMEventQuestTowerRewardGroupTable { get; }

    public EntityMEventQuestUnlockConditionTable EntityMEventQuestUnlockConditionTable { get; }

    public EntityMExploreTable EntityMExploreTable { get; }

    public EntityMExploreGradeAssetTable EntityMExploreGradeAssetTable { get; }

    public EntityMExploreGradeScoreTable EntityMExploreGradeScoreTable { get; }

    public EntityMExploreGroupTable EntityMExploreGroupTable { get; }

    public EntityMExploreUnlockConditionTable EntityMExploreUnlockConditionTable { get; }

    public EntityMExtraQuestGroupTable EntityMExtraQuestGroupTable { get; }

    public EntityMExtraQuestGroupInMainQuestChapterTable EntityMExtraQuestGroupInMainQuestChapterTable { get; }

    public EntityMFieldEffectBlessRelationTable EntityMFieldEffectBlessRelationTable { get; }

    public EntityMFieldEffectDecreasePointTable EntityMFieldEffectDecreasePointTable { get; }

    public EntityMFieldEffectGroupTable EntityMFieldEffectGroupTable { get; }

    public EntityMGachaMedalTable EntityMGachaMedalTable { get; }

    public EntityMGiftTextTable EntityMGiftTextTable { get; }

    public EntityMGimmickTable EntityMGimmickTable { get; }

    public EntityMGimmickAdditionalAssetTable EntityMGimmickAdditionalAssetTable { get; }

    public EntityMGimmickExtraQuestTable EntityMGimmickExtraQuestTable { get; }

    public EntityMGimmickGroupTable EntityMGimmickGroupTable { get; }

    public EntityMGimmickGroupEventLogTable EntityMGimmickGroupEventLogTable { get; }

    public EntityMGimmickIntervalTable EntityMGimmickIntervalTable { get; }

    public EntityMGimmickOrnamentTable EntityMGimmickOrnamentTable { get; }

    public EntityMGimmickSequenceTable EntityMGimmickSequenceTable { get; }

    public EntityMGimmickSequenceGroupTable EntityMGimmickSequenceGroupTable { get; }

    public EntityMGimmickSequenceRewardGroupTable EntityMGimmickSequenceRewardGroupTable { get; }

    public EntityMGimmickSequenceScheduleTable EntityMGimmickSequenceScheduleTable { get; }

    public EntityMHeadupDisplayViewTable EntityMHeadupDisplayViewTable { get; }

    public EntityMHelpTable EntityMHelpTable { get; }

    public EntityMHelpCategoryTable EntityMHelpCategoryTable { get; }

    public EntityMHelpItemTable EntityMHelpItemTable { get; }

    public EntityMHelpPageGroupTable EntityMHelpPageGroupTable { get; }

    public EntityMImportantItemTable EntityMImportantItemTable { get; }

    public EntityMImportantItemEffectTable EntityMImportantItemEffectTable { get; }

    public EntityMImportantItemEffectDropCountTable EntityMImportantItemEffectDropCountTable { get; }

    public EntityMImportantItemEffectDropRateTable EntityMImportantItemEffectDropRateTable { get; }

    public EntityMImportantItemEffectTargetItemGroupTable EntityMImportantItemEffectTargetItemGroupTable { get; }

    public EntityMImportantItemEffectTargetQuestGroupTable EntityMImportantItemEffectTargetQuestGroupTable { get; }

    public EntityMImportantItemEffectUnlockFunctionTable EntityMImportantItemEffectUnlockFunctionTable { get; }

    public EntityMLibraryEventQuestStoryGroupingTable EntityMLibraryEventQuestStoryGroupingTable { get; }

    public EntityMLibraryMainQuestGroupTable EntityMLibraryMainQuestGroupTable { get; }

    public EntityMLibraryMainQuestStoryTable EntityMLibraryMainQuestStoryTable { get; }

    public EntityMLibraryMovieTable EntityMLibraryMovieTable { get; }

    public EntityMLibraryMovieCategoryTable EntityMLibraryMovieCategoryTable { get; }

    public EntityMLibraryMovieUnlockConditionTable EntityMLibraryMovieUnlockConditionTable { get; }

    public EntityMLibraryRecordGroupingTable EntityMLibraryRecordGroupingTable { get; }

    public EntityMLibraryStoryGroupTable EntityMLibraryStoryGroupTable { get; }

    public EntityMLimitedOpenTextTable EntityMLimitedOpenTextTable { get; }

    public EntityMLimitedOpenTextGroupTable EntityMLimitedOpenTextGroupTable { get; }

    public EntityMListSettingAbilityGroupTable EntityMListSettingAbilityGroupTable { get; }

    public EntityMListSettingAbilityGroupTargetTable EntityMListSettingAbilityGroupTargetTable { get; }

    public EntityMLoginBonusTable EntityMLoginBonusTable { get; }

    public EntityMLoginBonusStampTable EntityMLoginBonusStampTable { get; }

    public EntityMMainQuestChapterTable EntityMMainQuestChapterTable { get; }

    public EntityMMainQuestPortalCageCharacterTable EntityMMainQuestPortalCageCharacterTable { get; }

    public EntityMMainQuestRouteTable EntityMMainQuestRouteTable { get; }

    public EntityMMainQuestRouteAnotherReplayFlowUnlockConditionTable EntityMMainQuestRouteAnotherReplayFlowUnlockConditionTable { get; }

    public EntityMMainQuestSeasonTable EntityMMainQuestSeasonTable { get; }

    public EntityMMainQuestSequenceTable EntityMMainQuestSequenceTable { get; }

    public EntityMMainQuestSequenceGroupTable EntityMMainQuestSequenceGroupTable { get; }

    public EntityMMaintenanceTable EntityMMaintenanceTable { get; }

    public EntityMMaintenanceGroupTable EntityMMaintenanceGroupTable { get; }

    public EntityMMaterialTable EntityMMaterialTable { get; }

    public EntityMMaterialSaleObtainPossessionTable EntityMMaterialSaleObtainPossessionTable { get; }

    public EntityMMissionTable EntityMMissionTable { get; }

    public EntityMMissionClearConditionValueViewTable EntityMMissionClearConditionValueViewTable { get; }

    public EntityMMissionGroupTable EntityMMissionGroupTable { get; }

    public EntityMMissionLinkTable EntityMMissionLinkTable { get; }

    public EntityMMissionPassTable EntityMMissionPassTable { get; }

    public EntityMMissionPassLevelGroupTable EntityMMissionPassLevelGroupTable { get; }

    public EntityMMissionPassMissionGroupTable EntityMMissionPassMissionGroupTable { get; }

    public EntityMMissionPassRewardGroupTable EntityMMissionPassRewardGroupTable { get; }

    public EntityMMissionRewardTable EntityMMissionRewardTable { get; }

    public EntityMMissionSubCategoryTextTable EntityMMissionSubCategoryTextTable { get; }

    public EntityMMissionTermTable EntityMMissionTermTable { get; }

    public EntityMMissionUnlockConditionTable EntityMMissionUnlockConditionTable { get; }

    public EntityMMomBannerTable EntityMMomBannerTable { get; }

    public EntityMMomPointBannerTable EntityMMomPointBannerTable { get; }

    public EntityMMovieTable EntityMMovieTable { get; }

    public EntityMNaviCutInTable EntityMNaviCutInTable { get; }

    public EntityMNaviCutInContentGroupTable EntityMNaviCutInContentGroupTable { get; }

    public EntityMNaviCutInTextTable EntityMNaviCutInTextTable { get; }

    public EntityMNumericalFunctionTable EntityMNumericalFunctionTable { get; }

    public EntityMNumericalFunctionParameterGroupTable EntityMNumericalFunctionParameterGroupTable { get; }

    public EntityMNumericalParameterMapTable EntityMNumericalParameterMapTable { get; }

    public EntityMOmikujiTable EntityMOmikujiTable { get; }

    public EntityMOverrideHitEffectConditionCriticalTable EntityMOverrideHitEffectConditionCriticalTable { get; }

    public EntityMOverrideHitEffectConditionDamageAttributeTable EntityMOverrideHitEffectConditionDamageAttributeTable { get; }

    public EntityMOverrideHitEffectConditionGroupTable EntityMOverrideHitEffectConditionGroupTable { get; }

    public EntityMOverrideHitEffectConditionSkillExecutorTable EntityMOverrideHitEffectConditionSkillExecutorTable { get; }

    public EntityMPartsTable EntityMPartsTable { get; }

    public EntityMPartsEnhancedTable EntityMPartsEnhancedTable { get; }

    public EntityMPartsEnhancedSubStatusTable EntityMPartsEnhancedSubStatusTable { get; }

    public EntityMPartsGroupTable EntityMPartsGroupTable { get; }

    public EntityMPartsLevelUpPriceGroupTable EntityMPartsLevelUpPriceGroupTable { get; }

    public EntityMPartsLevelUpRateGroupTable EntityMPartsLevelUpRateGroupTable { get; }

    public EntityMPartsRarityTable EntityMPartsRarityTable { get; }

    public EntityMPartsSeriesTable EntityMPartsSeriesTable { get; }

    public EntityMPartsSeriesBonusAbilityGroupTable EntityMPartsSeriesBonusAbilityGroupTable { get; }

    public EntityMPartsStatusMainTable EntityMPartsStatusMainTable { get; }

    public EntityMPlatformPaymentTable EntityMPlatformPaymentTable { get; }

    public EntityMPlatformPaymentPriceTable EntityMPlatformPaymentPriceTable { get; }

    public EntityMPortalCageAccessPointFunctionGroupTable EntityMPortalCageAccessPointFunctionGroupTable { get; }

    public EntityMPortalCageAccessPointFunctionGroupScheduleTable EntityMPortalCageAccessPointFunctionGroupScheduleTable { get; }

    public EntityMPortalCageCharacterGroupTable EntityMPortalCageCharacterGroupTable { get; }

    public EntityMPortalCageGateTable EntityMPortalCageGateTable { get; }

    public EntityMPortalCageSceneTable EntityMPortalCageSceneTable { get; }

    public EntityMPossessionAcquisitionRouteTable EntityMPossessionAcquisitionRouteTable { get; }

    public EntityMPowerCalculationConstantValueTable EntityMPowerCalculationConstantValueTable { get; }

    public EntityMPowerReferenceStatusGroupTable EntityMPowerReferenceStatusGroupTable { get; }

    public EntityMPremiumItemTable EntityMPremiumItemTable { get; }

    public EntityMPvpBackgroundTable EntityMPvpBackgroundTable { get; }

    public EntityMPvpGradeTable EntityMPvpGradeTable { get; }

    public EntityMPvpGradeGroupTable EntityMPvpGradeGroupTable { get; }

    public EntityMPvpGradeOneMatchRewardTable EntityMPvpGradeOneMatchRewardTable { get; }

    public EntityMPvpGradeOneMatchRewardGroupTable EntityMPvpGradeOneMatchRewardGroupTable { get; }

    public EntityMPvpGradeWeeklyRewardGroupTable EntityMPvpGradeWeeklyRewardGroupTable { get; }

    public EntityMPvpRewardTable EntityMPvpRewardTable { get; }

    public EntityMPvpSeasonTable EntityMPvpSeasonTable { get; }

    public EntityMPvpSeasonGradeTable EntityMPvpSeasonGradeTable { get; }

    public EntityMPvpSeasonGroupingTable EntityMPvpSeasonGroupingTable { get; }

    public EntityMPvpSeasonRankRewardTable EntityMPvpSeasonRankRewardTable { get; }

    public EntityMPvpSeasonRankRewardGroupTable EntityMPvpSeasonRankRewardGroupTable { get; }

    public EntityMPvpSeasonRankRewardPerSeasonTable EntityMPvpSeasonRankRewardPerSeasonTable { get; }

    public EntityMPvpSeasonRankRewardRankGroupTable EntityMPvpSeasonRankRewardRankGroupTable { get; }

    public EntityMPvpWeeklyRankRewardGroupTable EntityMPvpWeeklyRankRewardGroupTable { get; }

    public EntityMPvpWeeklyRankRewardRankGroupTable EntityMPvpWeeklyRankRewardRankGroupTable { get; }

    public EntityMPvpWinStreakCountEffectTable EntityMPvpWinStreakCountEffectTable { get; }

    public EntityMQuestTable EntityMQuestTable { get; }

    public EntityMQuestBonusTable EntityMQuestBonusTable { get; }

    public EntityMQuestBonusAbilityTable EntityMQuestBonusAbilityTable { get; }

    public EntityMQuestBonusAllyCharacterTable EntityMQuestBonusAllyCharacterTable { get; }

    public EntityMQuestBonusCharacterGroupTable EntityMQuestBonusCharacterGroupTable { get; }

    public EntityMQuestBonusCostumeGroupTable EntityMQuestBonusCostumeGroupTable { get; }

    public EntityMQuestBonusCostumeSettingGroupTable EntityMQuestBonusCostumeSettingGroupTable { get; }

    public EntityMQuestBonusDropRewardTable EntityMQuestBonusDropRewardTable { get; }

    public EntityMQuestBonusEffectGroupTable EntityMQuestBonusEffectGroupTable { get; }

    public EntityMQuestBonusExpTable EntityMQuestBonusExpTable { get; }

    public EntityMQuestBonusTermGroupTable EntityMQuestBonusTermGroupTable { get; }

    public EntityMQuestBonusWeaponGroupTable EntityMQuestBonusWeaponGroupTable { get; }

    public EntityMQuestCampaignTable EntityMQuestCampaignTable { get; }

    public EntityMQuestCampaignEffectGroupTable EntityMQuestCampaignEffectGroupTable { get; }

    public EntityMQuestCampaignTargetGroupTable EntityMQuestCampaignTargetGroupTable { get; }

    public EntityMQuestCampaignTargetItemGroupTable EntityMQuestCampaignTargetItemGroupTable { get; }

    public EntityMQuestDeckMultiRestrictionGroupTable EntityMQuestDeckMultiRestrictionGroupTable { get; }

    public EntityMQuestDeckRestrictionGroupTable EntityMQuestDeckRestrictionGroupTable { get; }

    public EntityMQuestDeckRestrictionGroupUnlockTable EntityMQuestDeckRestrictionGroupUnlockTable { get; }

    public EntityMQuestDisplayAttributeGroupTable EntityMQuestDisplayAttributeGroupTable { get; }

    public EntityMQuestDisplayEnemyThumbnailReplaceTable EntityMQuestDisplayEnemyThumbnailReplaceTable { get; }

    public EntityMQuestFirstClearRewardGroupTable EntityMQuestFirstClearRewardGroupTable { get; }

    public EntityMQuestFirstClearRewardSwitchTable EntityMQuestFirstClearRewardSwitchTable { get; }

    public EntityMQuestMissionTable EntityMQuestMissionTable { get; }

    public EntityMQuestMissionConditionValueGroupTable EntityMQuestMissionConditionValueGroupTable { get; }

    public EntityMQuestMissionGroupTable EntityMQuestMissionGroupTable { get; }

    public EntityMQuestMissionRewardTable EntityMQuestMissionRewardTable { get; }

    public EntityMQuestPickupRewardGroupTable EntityMQuestPickupRewardGroupTable { get; }

    public EntityMQuestRelationMainFlowTable EntityMQuestRelationMainFlowTable { get; }

    public EntityMQuestReleaseConditionBigHuntScoreTable EntityMQuestReleaseConditionBigHuntScoreTable { get; }

    public EntityMQuestReleaseConditionCharacterLevelTable EntityMQuestReleaseConditionCharacterLevelTable { get; }

    public EntityMQuestReleaseConditionDeckPowerTable EntityMQuestReleaseConditionDeckPowerTable { get; }

    public EntityMQuestReleaseConditionGroupTable EntityMQuestReleaseConditionGroupTable { get; }

    public EntityMQuestReleaseConditionListTable EntityMQuestReleaseConditionListTable { get; }

    public EntityMQuestReleaseConditionQuestChallengeTable EntityMQuestReleaseConditionQuestChallengeTable { get; }

    public EntityMQuestReleaseConditionQuestClearTable EntityMQuestReleaseConditionQuestClearTable { get; }

    public EntityMQuestReleaseConditionUserLevelTable EntityMQuestReleaseConditionUserLevelTable { get; }

    public EntityMQuestReleaseConditionWeaponAcquisitionTable EntityMQuestReleaseConditionWeaponAcquisitionTable { get; }

    public EntityMQuestReplayFlowRewardGroupTable EntityMQuestReplayFlowRewardGroupTable { get; }

    public EntityMQuestSceneTable EntityMQuestSceneTable { get; }

    public EntityMQuestSceneBattleTable EntityMQuestSceneBattleTable { get; }

    public EntityMQuestSceneChoiceTable EntityMQuestSceneChoiceTable { get; }

    public EntityMQuestSceneChoiceCostumeEffectGroupTable EntityMQuestSceneChoiceCostumeEffectGroupTable { get; }

    public EntityMQuestSceneChoiceEffectTable EntityMQuestSceneChoiceEffectTable { get; }

    public EntityMQuestSceneChoiceWeaponEffectGroupTable EntityMQuestSceneChoiceWeaponEffectGroupTable { get; }

    public EntityMQuestSceneNotConfirmTitleDialogTable EntityMQuestSceneNotConfirmTitleDialogTable { get; }

    public EntityMQuestSceneOutgameBlendshapeMotionTable EntityMQuestSceneOutgameBlendshapeMotionTable { get; }

    public EntityMQuestScenePictureBookReplaceTable EntityMQuestScenePictureBookReplaceTable { get; }

    public EntityMQuestScheduleTable EntityMQuestScheduleTable { get; }

    public EntityMQuestScheduleCorrespondenceTable EntityMQuestScheduleCorrespondenceTable { get; }

    public EntityMReportTable EntityMReportTable { get; }

    public EntityMShopTable EntityMShopTable { get; }

    public EntityMShopDisplayPriceTable EntityMShopDisplayPriceTable { get; }

    public EntityMShopItemTable EntityMShopItemTable { get; }

    public EntityMShopItemAdditionalContentTable EntityMShopItemAdditionalContentTable { get; }

    public EntityMShopItemCellTable EntityMShopItemCellTable { get; }

    public EntityMShopItemCellGroupTable EntityMShopItemCellGroupTable { get; }

    public EntityMShopItemCellLimitedOpenTable EntityMShopItemCellLimitedOpenTable { get; }

    public EntityMShopItemCellTermTable EntityMShopItemCellTermTable { get; }

    public EntityMShopItemContentEffectTable EntityMShopItemContentEffectTable { get; }

    public EntityMShopItemContentMissionTable EntityMShopItemContentMissionTable { get; }

    public EntityMShopItemContentPossessionTable EntityMShopItemContentPossessionTable { get; }

    public EntityMShopItemLimitedStockTable EntityMShopItemLimitedStockTable { get; }

    public EntityMShopItemUserLevelConditionTable EntityMShopItemUserLevelConditionTable { get; }

    public EntityMShopReplaceableGemTable EntityMShopReplaceableGemTable { get; }

    public EntityMSideStoryQuestTable EntityMSideStoryQuestTable { get; }

    public EntityMSideStoryQuestLimitContentTable EntityMSideStoryQuestLimitContentTable { get; }

    public EntityMSideStoryQuestSceneTable EntityMSideStoryQuestSceneTable { get; }

    public EntityMSkillTable EntityMSkillTable { get; }

    public EntityMSkillAbnormalTable EntityMSkillAbnormalTable { get; }

    public EntityMSkillAbnormalBehaviourTable EntityMSkillAbnormalBehaviourTable { get; }

    public EntityMSkillAbnormalBehaviourActionAbnormalResistanceTable EntityMSkillAbnormalBehaviourActionAbnormalResistanceTable { get; }

    public EntityMSkillAbnormalBehaviourActionAttributeDamageCorrectionTable EntityMSkillAbnormalBehaviourActionAttributeDamageCorrectionTable { get; }

    public EntityMSkillAbnormalBehaviourActionBuffResistanceTable EntityMSkillAbnormalBehaviourActionBuffResistanceTable { get; }

    public EntityMSkillAbnormalBehaviourActionDamageTable EntityMSkillAbnormalBehaviourActionDamageTable { get; }

    public EntityMSkillAbnormalBehaviourActionDamageMultiplyTable EntityMSkillAbnormalBehaviourActionDamageMultiplyTable { get; }

    public EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlwaysTable EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlwaysTable { get; }

    public EntityMSkillAbnormalBehaviourActionDefaultSkillLotteryTable EntityMSkillAbnormalBehaviourActionDefaultSkillLotteryTable { get; }

    public EntityMSkillAbnormalBehaviourActionHitRatioDownTable EntityMSkillAbnormalBehaviourActionHitRatioDownTable { get; }

    public EntityMSkillAbnormalBehaviourActionModifyHateValueTable EntityMSkillAbnormalBehaviourActionModifyHateValueTable { get; }

    public EntityMSkillAbnormalBehaviourActionOverrideEvasionValueTable EntityMSkillAbnormalBehaviourActionOverrideEvasionValueTable { get; }

    public EntityMSkillAbnormalBehaviourActionOverrideHitEffectTable EntityMSkillAbnormalBehaviourActionOverrideHitEffectTable { get; }

    public EntityMSkillAbnormalBehaviourActionRecoveryTable EntityMSkillAbnormalBehaviourActionRecoveryTable { get; }

    public EntityMSkillAbnormalBehaviourActionTurnRestrictionTable EntityMSkillAbnormalBehaviourActionTurnRestrictionTable { get; }

    public EntityMSkillAbnormalBehaviourGroupTable EntityMSkillAbnormalBehaviourGroupTable { get; }

    public EntityMSkillAbnormalDamageMultiplyDetailAbnormalTable EntityMSkillAbnormalDamageMultiplyDetailAbnormalTable { get; }

    public EntityMSkillAbnormalDamageMultiplyDetailBuffAttachedTable EntityMSkillAbnormalDamageMultiplyDetailBuffAttachedTable { get; }

    public EntityMSkillAbnormalDamageMultiplyDetailCriticalTable EntityMSkillAbnormalDamageMultiplyDetailCriticalTable { get; }

    public EntityMSkillAbnormalDamageMultiplyDetailHitIndexTable EntityMSkillAbnormalDamageMultiplyDetailHitIndexTable { get; }

    public EntityMSkillAbnormalDamageMultiplyDetailSkillfulWeaponTable EntityMSkillAbnormalDamageMultiplyDetailSkillfulWeaponTable { get; }

    public EntityMSkillAbnormalLifetimeTable EntityMSkillAbnormalLifetimeTable { get; }

    public EntityMSkillAbnormalLifetimeBehaviourActivateCountTable EntityMSkillAbnormalLifetimeBehaviourActivateCountTable { get; }

    public EntityMSkillAbnormalLifetimeBehaviourFrameCountTable EntityMSkillAbnormalLifetimeBehaviourFrameCountTable { get; }

    public EntityMSkillAbnormalLifetimeBehaviourGroupTable EntityMSkillAbnormalLifetimeBehaviourGroupTable { get; }

    public EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCountTable EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCountTable { get; }

    public EntityMSkillAbnormalLifetimeBehaviourTurnCountTable EntityMSkillAbnormalLifetimeBehaviourTurnCountTable { get; }

    public EntityMSkillBehaviourTable EntityMSkillBehaviourTable { get; }

    public EntityMSkillBehaviourActionAbnormalTable EntityMSkillBehaviourActionAbnormalTable { get; }

    public EntityMSkillBehaviourActionActiveSkillDamageCorrectionTable EntityMSkillBehaviourActionActiveSkillDamageCorrectionTable { get; }

    public EntityMSkillBehaviourActionAdvanceActiveSkillCooltimeTable EntityMSkillBehaviourActionAdvanceActiveSkillCooltimeTable { get; }

    public EntityMSkillBehaviourActionAdvanceActiveSkillCooltimeImmediateTable EntityMSkillBehaviourActionAdvanceActiveSkillCooltimeImmediateTable { get; }

    public EntityMSkillBehaviourActionAttackTable EntityMSkillBehaviourActionAttackTable { get; }

    public EntityMSkillBehaviourActionAttackClampHpTable EntityMSkillBehaviourActionAttackClampHpTable { get; }

    public EntityMSkillBehaviourActionAttackComboTable EntityMSkillBehaviourActionAttackComboTable { get; }

    public EntityMSkillBehaviourActionAttackFixedDamageTable EntityMSkillBehaviourActionAttackFixedDamageTable { get; }

    public EntityMSkillBehaviourActionAttackHpRatioTable EntityMSkillBehaviourActionAttackHpRatioTable { get; }

    public EntityMSkillBehaviourActionAttackIgnoreVitalityTable EntityMSkillBehaviourActionAttackIgnoreVitalityTable { get; }

    public EntityMSkillBehaviourActionAttackMainWeaponAttributeTable EntityMSkillBehaviourActionAttackMainWeaponAttributeTable { get; }

    public EntityMSkillBehaviourActionAttackSkillfulMainWeaponTypeTable EntityMSkillBehaviourActionAttackSkillfulMainWeaponTypeTable { get; }

    public EntityMSkillBehaviourActionAttackVitalityTable EntityMSkillBehaviourActionAttackVitalityTable { get; }

    public EntityMSkillBehaviourActionAttributeDamageCorrectionTable EntityMSkillBehaviourActionAttributeDamageCorrectionTable { get; }

    public EntityMSkillBehaviourActionBuffTable EntityMSkillBehaviourActionBuffTable { get; }

    public EntityMSkillBehaviourActionChangestepTable EntityMSkillBehaviourActionChangestepTable { get; }

    public EntityMSkillBehaviourActionDamageCorrectionHpRatioTable EntityMSkillBehaviourActionDamageCorrectionHpRatioTable { get; }

    public EntityMSkillBehaviourActionDamageMultiplyTable EntityMSkillBehaviourActionDamageMultiplyTable { get; }

    public EntityMSkillBehaviourActionDefaultSkillLotteryTable EntityMSkillBehaviourActionDefaultSkillLotteryTable { get; }

    public EntityMSkillBehaviourActionExtendBuffCooltimeTable EntityMSkillBehaviourActionExtendBuffCooltimeTable { get; }

    public EntityMSkillBehaviourActionHpRatioDamageTable EntityMSkillBehaviourActionHpRatioDamageTable { get; }

    public EntityMSkillBehaviourActionOverlimitDamageMultiplyTable EntityMSkillBehaviourActionOverlimitDamageMultiplyTable { get; }

    public EntityMSkillBehaviourActionRecoveryTable EntityMSkillBehaviourActionRecoveryTable { get; }

    public EntityMSkillBehaviourActionRecoveryPointCorrectionTable EntityMSkillBehaviourActionRecoveryPointCorrectionTable { get; }

    public EntityMSkillBehaviourActionRemoveAbnormalTable EntityMSkillBehaviourActionRemoveAbnormalTable { get; }

    public EntityMSkillBehaviourActionRemoveBuffTable EntityMSkillBehaviourActionRemoveBuffTable { get; }

    public EntityMSkillBehaviourActionShortenActiveSkillCooltimeTable EntityMSkillBehaviourActionShortenActiveSkillCooltimeTable { get; }

    public EntityMSkillBehaviourActionSkillRecoveryPowerCorrectionTable EntityMSkillBehaviourActionSkillRecoveryPowerCorrectionTable { get; }

    public EntityMSkillBehaviourActivationConditionActivationUpperCountTable EntityMSkillBehaviourActivationConditionActivationUpperCountTable { get; }

    public EntityMSkillBehaviourActivationConditionAttributeTable EntityMSkillBehaviourActivationConditionAttributeTable { get; }

    public EntityMSkillBehaviourActivationConditionGroupTable EntityMSkillBehaviourActivationConditionGroupTable { get; }

    public EntityMSkillBehaviourActivationConditionHpRatioTable EntityMSkillBehaviourActivationConditionHpRatioTable { get; }

    public EntityMSkillBehaviourActivationConditionInSkillFlowTable EntityMSkillBehaviourActivationConditionInSkillFlowTable { get; }

    public EntityMSkillBehaviourActivationConditionWaveNumberTable EntityMSkillBehaviourActivationConditionWaveNumberTable { get; }

    public EntityMSkillBehaviourActivationMethodTable EntityMSkillBehaviourActivationMethodTable { get; }

    public EntityMSkillBehaviourGroupTable EntityMSkillBehaviourGroupTable { get; }

    public EntityMSkillBuffTable EntityMSkillBuffTable { get; }

    public EntityMSkillCasttimeTable EntityMSkillCasttimeTable { get; }

    public EntityMSkillCasttimeBehaviourTable EntityMSkillCasttimeBehaviourTable { get; }

    public EntityMSkillCasttimeBehaviourActionOnFrameUpdateTable EntityMSkillCasttimeBehaviourActionOnFrameUpdateTable { get; }

    public EntityMSkillCasttimeBehaviourActionOnSkillDamageConditionTable EntityMSkillCasttimeBehaviourActionOnSkillDamageConditionTable { get; }

    public EntityMSkillCasttimeBehaviourGroupTable EntityMSkillCasttimeBehaviourGroupTable { get; }

    public EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroupTable EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroupTable { get; }

    public EntityMSkillCooltimeBehaviourTable EntityMSkillCooltimeBehaviourTable { get; }

    public EntityMSkillCooltimeBehaviourGroupTable EntityMSkillCooltimeBehaviourGroupTable { get; }

    public EntityMSkillCooltimeBehaviourOnExecuteActiveSkillTable EntityMSkillCooltimeBehaviourOnExecuteActiveSkillTable { get; }

    public EntityMSkillCooltimeBehaviourOnExecuteCompanionSkillTable EntityMSkillCooltimeBehaviourOnExecuteCompanionSkillTable { get; }

    public EntityMSkillCooltimeBehaviourOnExecuteDefaultSkillTable EntityMSkillCooltimeBehaviourOnExecuteDefaultSkillTable { get; }

    public EntityMSkillCooltimeBehaviourOnFrameUpdateTable EntityMSkillCooltimeBehaviourOnFrameUpdateTable { get; }

    public EntityMSkillCooltimeBehaviourOnSkillDamageTable EntityMSkillCooltimeBehaviourOnSkillDamageTable { get; }

    public EntityMSkillDamageMultiplyAbnormalAttachedValueGroupTable EntityMSkillDamageMultiplyAbnormalAttachedValueGroupTable { get; }

    public EntityMSkillDamageMultiplyDetailAbnormalAttachedTable EntityMSkillDamageMultiplyDetailAbnormalAttachedTable { get; }

    public EntityMSkillDamageMultiplyDetailAlwaysTable EntityMSkillDamageMultiplyDetailAlwaysTable { get; }

    public EntityMSkillDamageMultiplyDetailBuffAttachedTable EntityMSkillDamageMultiplyDetailBuffAttachedTable { get; }

    public EntityMSkillDamageMultiplyDetailCriticalTable EntityMSkillDamageMultiplyDetailCriticalTable { get; }

    public EntityMSkillDamageMultiplyDetailHitIndexTable EntityMSkillDamageMultiplyDetailHitIndexTable { get; }

    public EntityMSkillDamageMultiplyDetailSkillfulWeaponTypeTable EntityMSkillDamageMultiplyDetailSkillfulWeaponTypeTable { get; }

    public EntityMSkillDamageMultiplyDetailSpecifiedCostumeTypeTable EntityMSkillDamageMultiplyDetailSpecifiedCostumeTypeTable { get; }

    public EntityMSkillDamageMultiplyHitIndexValueGroupTable EntityMSkillDamageMultiplyHitIndexValueGroupTable { get; }

    public EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroupTable EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroupTable { get; }

    public EntityMSkillDetailTable EntityMSkillDetailTable { get; }

    public EntityMSkillLevelGroupTable EntityMSkillLevelGroupTable { get; }

    public EntityMSkillRemoveAbnormalTargetAbnormalGroupTable EntityMSkillRemoveAbnormalTargetAbnormalGroupTable { get; }

    public EntityMSkillRemoveBuffFilterStatusKindTable EntityMSkillRemoveBuffFilterStatusKindTable { get; }

    public EntityMSkillReserveUiTypeTable EntityMSkillReserveUiTypeTable { get; }

    public EntityMSmartphoneChatGroupTable EntityMSmartphoneChatGroupTable { get; }

    public EntityMSmartphoneChatGroupMessageTable EntityMSmartphoneChatGroupMessageTable { get; }

    public EntityMSpeakerTable EntityMSpeakerTable { get; }

    public EntityMStainedGlassTable EntityMStainedGlassTable { get; }

    public EntityMStainedGlassStatusUpGroupTable EntityMStainedGlassStatusUpGroupTable { get; }

    public EntityMStainedGlassStatusUpTargetGroupTable EntityMStainedGlassStatusUpTargetGroupTable { get; }

    public EntityMThoughtTable EntityMThoughtTable { get; }

    public EntityMTipTable EntityMTipTable { get; }

    public EntityMTipBackgroundAssetTable EntityMTipBackgroundAssetTable { get; }

    public EntityMTipDisplayConditionGroupTable EntityMTipDisplayConditionGroupTable { get; }

    public EntityMTipGroupTable EntityMTipGroupTable { get; }

    public EntityMTipGroupBackgroundAssetTable EntityMTipGroupBackgroundAssetTable { get; }

    public EntityMTipGroupBackgroundAssetRelationTable EntityMTipGroupBackgroundAssetRelationTable { get; }

    public EntityMTipGroupSelectionTable EntityMTipGroupSelectionTable { get; }

    public EntityMTipGroupSituationTable EntityMTipGroupSituationTable { get; }

    public EntityMTipGroupSituationSeasonTable EntityMTipGroupSituationSeasonTable { get; }

    public EntityMTitleFlowMovieTable EntityMTitleFlowMovieTable { get; }

    public EntityMTitleStillTable EntityMTitleStillTable { get; }

    public EntityMTitleStillGroupTable EntityMTitleStillGroupTable { get; }

    public EntityMTutorialConsumePossessionGroupTable EntityMTutorialConsumePossessionGroupTable { get; }

    public EntityMTutorialDialogTable EntityMTutorialDialogTable { get; }

    public EntityMTutorialUnlockConditionTable EntityMTutorialUnlockConditionTable { get; }

    public EntityMUserLevelTable EntityMUserLevelTable { get; }

    public EntityMUserQuestSceneGrantPossessionTable EntityMUserQuestSceneGrantPossessionTable { get; }

    public EntityMWeaponTable EntityMWeaponTable { get; }

    public EntityMWeaponAbilityEnhancementMaterialTable EntityMWeaponAbilityEnhancementMaterialTable { get; }

    public EntityMWeaponAbilityGroupTable EntityMWeaponAbilityGroupTable { get; }

    public EntityMWeaponAwakenTable EntityMWeaponAwakenTable { get; }

    public EntityMWeaponAwakenAbilityTable EntityMWeaponAwakenAbilityTable { get; }

    public EntityMWeaponAwakenEffectGroupTable EntityMWeaponAwakenEffectGroupTable { get; }

    public EntityMWeaponAwakenMaterialGroupTable EntityMWeaponAwakenMaterialGroupTable { get; }

    public EntityMWeaponAwakenStatusUpGroupTable EntityMWeaponAwakenStatusUpGroupTable { get; }

    public EntityMWeaponBaseStatusTable EntityMWeaponBaseStatusTable { get; }

    public EntityMWeaponConsumeExchangeConsumableItemGroupTable EntityMWeaponConsumeExchangeConsumableItemGroupTable { get; }

    public EntityMWeaponEnhancedTable EntityMWeaponEnhancedTable { get; }

    public EntityMWeaponEnhancedAbilityTable EntityMWeaponEnhancedAbilityTable { get; }

    public EntityMWeaponEnhancedSkillTable EntityMWeaponEnhancedSkillTable { get; }

    public EntityMWeaponEvolutionGroupTable EntityMWeaponEvolutionGroupTable { get; }

    public EntityMWeaponEvolutionMaterialGroupTable EntityMWeaponEvolutionMaterialGroupTable { get; }

    public EntityMWeaponFieldEffectDecreasePointTable EntityMWeaponFieldEffectDecreasePointTable { get; }

    public EntityMWeaponRarityTable EntityMWeaponRarityTable { get; }

    public EntityMWeaponRarityLimitBreakMaterialGroupTable EntityMWeaponRarityLimitBreakMaterialGroupTable { get; }

    public EntityMWeaponSkillEnhancementMaterialTable EntityMWeaponSkillEnhancementMaterialTable { get; }

    public EntityMWeaponSkillGroupTable EntityMWeaponSkillGroupTable { get; }

    public EntityMWeaponSpecificEnhanceTable EntityMWeaponSpecificEnhanceTable { get; }

    public EntityMWeaponSpecificLimitBreakMaterialGroupTable EntityMWeaponSpecificLimitBreakMaterialGroupTable { get; }

    public EntityMWeaponStatusCalculationTable EntityMWeaponStatusCalculationTable { get; }

    public EntityMWeaponStoryReleaseConditionGroupTable EntityMWeaponStoryReleaseConditionGroupTable { get; }

    public EntityMWeaponStoryReleaseConditionOperationTable EntityMWeaponStoryReleaseConditionOperationTable { get; }

    public EntityMWeaponStoryReleaseConditionOperationGroupTable EntityMWeaponStoryReleaseConditionOperationGroupTable { get; }

    public EntityMWebviewMissionTable EntityMWebviewMissionTable { get; }

    public EntityMWebviewMissionTitleTextTable EntityMWebviewMissionTitleTextTable { get; }

    public EntityMWebviewPanelMissionTable EntityMWebviewPanelMissionTable { get; }

    public EntityMWebviewPanelMissionCompleteFlavorTextTable EntityMWebviewPanelMissionCompleteFlavorTextTable { get; }

    public EntityMWebviewPanelMissionPageTable EntityMWebviewPanelMissionPageTable { get; }
}
