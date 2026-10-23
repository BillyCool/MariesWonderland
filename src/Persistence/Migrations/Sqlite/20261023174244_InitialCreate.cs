using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MariesWonderland.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Uuid = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Uuid);
                });

            migrationBuilder.CreateTable(
                name: "IUser",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    OsType = table.Column<int>(type: "INTEGER", nullable: false),
                    PlatformType = table.Column<int>(type: "INTEGER", nullable: false),
                    UserRestrictionType = table.Column<int>(type: "INTEGER", nullable: false),
                    RegisterDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    GameStartDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    BirthYear = table.Column<int>(type: "INTEGER", nullable: false),
                    BirthMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUser", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserApple",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    AppleId = table.Column<string>(type: "TEXT", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserApple", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserAutoSaleSettingDetail",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PossessionAutoSaleItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    PossessionAutoSaleItemValue = table.Column<string>(type: "TEXT", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserAutoSaleSettingDetail", x => new { x.UserId, x.PossessionAutoSaleItemType });
                });

            migrationBuilder.CreateTable(
                name: "IUserBeginnerCampaign",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BeginnerCampaignId = table.Column<int>(type: "INTEGER", nullable: false),
                    CampaignRegisterDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserBeginnerCampaign", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserBigHuntMaxScore",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BigHuntBossId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxScore = table.Column<long>(type: "INTEGER", nullable: false),
                    MaxScoreUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserBigHuntMaxScore", x => new { x.UserId, x.BigHuntBossId });
                });

            migrationBuilder.CreateTable(
                name: "IUserBigHuntProgressStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentBigHuntBossQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentBigHuntQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDryRun = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserBigHuntProgressStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserBigHuntScheduleMaxScore",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BigHuntScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    BigHuntBossId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxScore = table.Column<long>(type: "INTEGER", nullable: false),
                    MaxScoreUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserBigHuntScheduleMaxScore", x => new { x.UserId, x.BigHuntScheduleId, x.BigHuntBossId });
                });

            migrationBuilder.CreateTable(
                name: "IUserBigHuntStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BigHuntBossQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyChallengeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestChallengeDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserBigHuntStatus", x => new { x.UserId, x.BigHuntBossQuestId });
                });

            migrationBuilder.CreateTable(
                name: "IUserBigHuntWeeklyMaxScore",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BigHuntWeeklyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    AttributeType = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxScore = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserBigHuntWeeklyMaxScore", x => new { x.UserId, x.BigHuntWeeklyVersion, x.AttributeType });
                });

            migrationBuilder.CreateTable(
                name: "IUserBigHuntWeeklyStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BigHuntWeeklyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    IsReceivedWeeklyReward = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserBigHuntWeeklyStatus", x => new { x.UserId, x.BigHuntWeeklyVersion });
                });

            migrationBuilder.CreateTable(
                name: "IUserCageOrnamentReward",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CageOrnamentId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCageOrnamentReward", x => new { x.UserId, x.CageOrnamentId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacter",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Exp = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacter", x => new { x.UserId, x.CharacterId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacterBoard",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterBoardId = table.Column<int>(type: "INTEGER", nullable: false),
                    PanelReleaseBit1 = table.Column<int>(type: "INTEGER", nullable: false),
                    PanelReleaseBit2 = table.Column<int>(type: "INTEGER", nullable: false),
                    PanelReleaseBit3 = table.Column<int>(type: "INTEGER", nullable: false),
                    PanelReleaseBit4 = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacterBoard", x => new { x.UserId, x.CharacterBoardId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacterBoardAbility",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    AbilityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacterBoardAbility", x => new { x.UserId, x.CharacterId, x.AbilityId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacterBoardCompleteReward",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterBoardCompleteRewardId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacterBoardCompleteReward", x => new { x.UserId, x.CharacterBoardCompleteRewardId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacterBoardStatusUp",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusCalculationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Hp = table.Column<int>(type: "INTEGER", nullable: false),
                    Attack = table.Column<int>(type: "INTEGER", nullable: false),
                    Vitality = table.Column<int>(type: "INTEGER", nullable: false),
                    Agility = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalRatio = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalAttack = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacterBoardStatusUp", x => new { x.UserId, x.CharacterId, x.StatusCalculationType });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacterCostumeLevelBonus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusCalculationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Hp = table.Column<int>(type: "INTEGER", nullable: false),
                    Attack = table.Column<int>(type: "INTEGER", nullable: false),
                    Vitality = table.Column<int>(type: "INTEGER", nullable: false),
                    Agility = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalRatio = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalAttack = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacterCostumeLevelBonus", x => new { x.UserId, x.CharacterId, x.StatusCalculationType });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacterRebirth",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    RebirthCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacterRebirth", x => new { x.UserId, x.CharacterId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCharacterViewerField",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterViewerFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReleaseDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCharacterViewerField", x => new { x.UserId, x.CharacterViewerFieldId });
                });

            migrationBuilder.CreateTable(
                name: "IUserComebackCampaign",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ComebackCampaignId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComebackDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserComebackCampaign", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserCompanion",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCompanionUuid = table.Column<string>(type: "TEXT", nullable: false),
                    CompanionId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadupDisplayViewId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCompanion", x => new { x.UserId, x.UserCompanionUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserConsumableItem",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ConsumableItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstAcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserConsumableItem", x => new { x.UserId, x.ConsumableItemId });
                });

            migrationBuilder.CreateTable(
                name: "IUserContentsStory",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentsStoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserContentsStory", x => new { x.UserId, x.ContentsStoryId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostume",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    CostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LimitBreakCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Exp = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadupDisplayViewId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    AwakenCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostume", x => new { x.UserId, x.UserCostumeUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostumeActiveSkill",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostumeActiveSkill", x => new { x.UserId, x.UserCostumeUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostumeAwakenStatusUp",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    StatusCalculationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Hp = table.Column<int>(type: "INTEGER", nullable: false),
                    Attack = table.Column<int>(type: "INTEGER", nullable: false),
                    Vitality = table.Column<int>(type: "INTEGER", nullable: false),
                    Agility = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalRatio = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalAttack = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostumeAwakenStatusUp", x => new { x.UserId, x.UserCostumeUuid, x.StatusCalculationType });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostumeLevelBonusReleaseStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastReleasedBonusLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfirmedBonusLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostumeLevelBonusReleaseStatus", x => new { x.UserId, x.CostumeId });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostumeLotteryEffect",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    OddsNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostumeLotteryEffect", x => new { x.UserId, x.UserCostumeUuid, x.SlotNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostumeLotteryEffectAbility",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    AbilityId = table.Column<int>(type: "INTEGER", nullable: false),
                    AbilityLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostumeLotteryEffectAbility", x => new { x.UserId, x.UserCostumeUuid, x.SlotNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostumeLotteryEffectPending",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    OddsNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostumeLotteryEffectPending", x => new { x.UserId, x.UserCostumeUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserCostumeLotteryEffectStatusUp",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    StatusCalculationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Hp = table.Column<int>(type: "INTEGER", nullable: false),
                    Attack = table.Column<int>(type: "INTEGER", nullable: false),
                    Vitality = table.Column<int>(type: "INTEGER", nullable: false),
                    Agility = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalRatio = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalAttack = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserCostumeLotteryEffectStatusUp", x => new { x.UserId, x.UserCostumeUuid, x.StatusCalculationType });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeck",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    DeckType = table.Column<int>(type: "INTEGER", nullable: false),
                    UserDeckNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    UserDeckCharacterUuid01 = table.Column<string>(type: "TEXT", nullable: false),
                    UserDeckCharacterUuid02 = table.Column<string>(type: "TEXT", nullable: false),
                    UserDeckCharacterUuid03 = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Power = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeck", x => new { x.UserId, x.DeckType, x.UserDeckNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeckCharacter",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserDeckCharacterUuid = table.Column<string>(type: "TEXT", nullable: false),
                    UserCostumeUuid = table.Column<string>(type: "TEXT", nullable: false),
                    MainUserWeaponUuid = table.Column<string>(type: "TEXT", nullable: false),
                    UserCompanionUuid = table.Column<string>(type: "TEXT", nullable: false),
                    Power = table.Column<int>(type: "INTEGER", nullable: false),
                    UserThoughtUuid = table.Column<string>(type: "TEXT", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeckCharacter", x => new { x.UserId, x.UserDeckCharacterUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeckCharacterDressupCostume",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserDeckCharacterUuid = table.Column<string>(type: "TEXT", nullable: false),
                    DressupCostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeckCharacterDressupCostume", x => new { x.UserId, x.UserDeckCharacterUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeckLimitContentDeletedCharacter",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserDeckNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    UserDeckCharacterNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeckLimitContentDeletedCharacter", x => new { x.UserId, x.UserDeckNumber, x.UserDeckCharacterNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeckLimitContentRestricted",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventQuestChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeckRestrictedUuid = table.Column<string>(type: "TEXT", nullable: false),
                    PossessionType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetUuid = table.Column<string>(type: "TEXT", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeckLimitContentRestricted", x => new { x.UserId, x.EventQuestChapterId, x.QuestId, x.DeckRestrictedUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeckPartsGroup",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserDeckCharacterUuid = table.Column<string>(type: "TEXT", nullable: false),
                    UserPartsUuid = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeckPartsGroup", x => new { x.UserId, x.UserDeckCharacterUuid, x.UserPartsUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeckSubWeaponGroup",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserDeckCharacterUuid = table.Column<string>(type: "TEXT", nullable: false),
                    UserWeaponUuid = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeckSubWeaponGroup", x => new { x.UserId, x.UserDeckCharacterUuid, x.UserWeaponUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserDeckTypeNote",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    DeckType = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxDeckPower = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDeckTypeNote", x => new { x.UserId, x.DeckType });
                });

            migrationBuilder.CreateTable(
                name: "IUserDokan",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    DokanId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserDokan", x => new { x.UserId, x.DokanId });
                });

            migrationBuilder.CreateTable(
                name: "IUserEventQuestDailyGroupCompleteReward",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    LastRewardReceiveEventQuestDailyGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastRewardReceiveDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserEventQuestDailyGroupCompleteReward", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserEventQuestGuerrillaFreeOpen",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    OpenMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyOpenedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserEventQuestGuerrillaFreeOpen", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserEventQuestLabyrinthSeason",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventQuestChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastJoinSeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSeasonRewardReceivedSeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserEventQuestLabyrinthSeason", x => new { x.UserId, x.EventQuestChapterId });
                });

            migrationBuilder.CreateTable(
                name: "IUserEventQuestLabyrinthStage",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventQuestChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    StageOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReceivedStageClearReward = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccumulationRewardReceivedQuestMissionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserEventQuestLabyrinthStage", x => new { x.UserId, x.EventQuestChapterId, x.StageOrder });
                });

            migrationBuilder.CreateTable(
                name: "IUserEventQuestProgressStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentEventQuestChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserEventQuestProgressStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserEventQuestTowerAccumulationReward",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventQuestChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestRewardReceiveQuestMissionClearCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserEventQuestTowerAccumulationReward", x => new { x.UserId, x.EventQuestChapterId });
                });

            migrationBuilder.CreateTable(
                name: "IUserExplore",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsUseExploreTicket = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayingExploreId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestPlayDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserExplore", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserExploreScore",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ExploreId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxScoreUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserExploreScore", x => new { x.UserId, x.ExploreId });
                });

            migrationBuilder.CreateTable(
                name: "IUserExtraQuestProgressStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserExtraQuestProgressStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserFacebook",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    FacebookId = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserFacebook", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserGem",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PaidGem = table.Column<int>(type: "INTEGER", nullable: false),
                    FreeGem = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserGem", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserGimmick",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GimmickSequenceScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickSequenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsGimmickCleared = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserGimmick", x => new { x.UserId, x.GimmickSequenceScheduleId, x.GimmickSequenceId, x.GimmickId });
                });

            migrationBuilder.CreateTable(
                name: "IUserGimmickOrnamentProgress",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GimmickSequenceScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickSequenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickOrnamentIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressValueBit = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserGimmickOrnamentProgress", x => new { x.UserId, x.GimmickSequenceScheduleId, x.GimmickSequenceId, x.GimmickId, x.GimmickOrnamentIndex });
                });

            migrationBuilder.CreateTable(
                name: "IUserGimmickSequence",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GimmickSequenceScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickSequenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsGimmickSequenceCleared = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClearDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserGimmickSequence", x => new { x.UserId, x.GimmickSequenceScheduleId });
                });

            migrationBuilder.CreateTable(
                name: "IUserGimmickUnlock",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GimmickSequenceScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickSequenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    GimmickId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUnlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserGimmickUnlock", x => new { x.UserId, x.GimmickSequenceScheduleId, x.GimmickSequenceId, x.GimmickId });
                });

            migrationBuilder.CreateTable(
                name: "IUserImportantItem",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ImportantItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstAcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserImportantItem", x => new { x.UserId, x.ImportantItemId });
                });

            migrationBuilder.CreateTable(
                name: "IUserLimitedOpen",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    LimitedOpenTargetType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    CloseDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserLimitedOpen", x => new { x.UserId, x.LimitedOpenTargetType, x.TargetId });
                });

            migrationBuilder.CreateTable(
                name: "IUserLogin",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalLoginCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ContinualLoginCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxContinualLoginCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastLoginDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LastComebackLoginDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserLogin", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserLoginBonus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    LoginBonusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentPageNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStampNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestRewardReceiveDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserLoginBonus", x => new { x.UserId, x.LoginBonusId });
                });

            migrationBuilder.CreateTable(
                name: "IUserMainQuestFlowStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentQuestFlowType = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMainQuestFlowStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserMainQuestMainFlowStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentMainQuestRouteId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReachedLastQuestScene = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMainQuestMainFlowStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserMainQuestProgressStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestFlowType = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMainQuestProgressStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserMainQuestReplayFlowStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentHeadQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMainQuestReplayFlowStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserMainQuestSeasonRoute",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MainQuestSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    MainQuestRouteId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMainQuestSeasonRoute", x => new { x.UserId, x.MainQuestSeasonId });
                });

            migrationBuilder.CreateTable(
                name: "IUserMaterial",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstAcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMaterial", x => new { x.UserId, x.MaterialId });
                });

            migrationBuilder.CreateTable(
                name: "IUserMission",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    ProgressValue = table.Column<int>(type: "INTEGER", nullable: false),
                    MissionProgressStatusType = table.Column<int>(type: "INTEGER", nullable: false),
                    ClearDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMission", x => new { x.UserId, x.MissionId });
                });

            migrationBuilder.CreateTable(
                name: "IUserMissionCompletionProgress",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressValue = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMissionCompletionProgress", x => new { x.UserId, x.MissionId });
                });

            migrationBuilder.CreateTable(
                name: "IUserMissionPassPoint",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MissionPassId = table.Column<int>(type: "INTEGER", nullable: false),
                    Point = table.Column<int>(type: "INTEGER", nullable: false),
                    PremiumRewardReceivedLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    NoPremiumRewardReceivedLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMissionPassPoint", x => new { x.UserId, x.MissionPassId });
                });

            migrationBuilder.CreateTable(
                name: "IUserMovie",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestViewedDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserMovie", x => new { x.UserId, x.MovieId });
                });

            migrationBuilder.CreateTable(
                name: "IUserNaviCutIn",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    NaviCutInId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserNaviCutIn", x => new { x.UserId, x.NaviCutInId });
                });

            migrationBuilder.CreateTable(
                name: "IUserOmikuji",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    OmikujiId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestDrawDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserOmikuji", x => new { x.UserId, x.OmikujiId });
                });

            migrationBuilder.CreateTable(
                name: "IUserParts",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserPartsUuid = table.Column<string>(type: "TEXT", nullable: false),
                    PartsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    PartsStatusMainId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsProtected = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserParts", x => new { x.UserId, x.UserPartsUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserPartsGroupNote",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PartsGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstAcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPartsGroupNote", x => new { x.UserId, x.PartsGroupId });
                });

            migrationBuilder.CreateTable(
                name: "IUserPartsPreset",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserPartsPresetNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    UserPartsUuid01 = table.Column<string>(type: "TEXT", nullable: false),
                    UserPartsUuid02 = table.Column<string>(type: "TEXT", nullable: false),
                    UserPartsUuid03 = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UserPartsPresetTagNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPartsPreset", x => new { x.UserId, x.UserPartsPresetNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserPartsPresetTag",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserPartsPresetTagNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPartsPresetTag", x => new { x.UserId, x.UserPartsPresetTagNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserPartsStatusSub",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserPartsUuid = table.Column<string>(type: "TEXT", nullable: false),
                    StatusIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    PartsStatusSubLotteryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusKindType = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusCalculationType = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusChangeValue = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPartsStatusSub", x => new { x.UserId, x.UserPartsUuid, x.StatusIndex });
                });

            migrationBuilder.CreateTable(
                name: "IUserPortalCageStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsCurrentProgress = table.Column<bool>(type: "INTEGER", nullable: false),
                    DropItemStartDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentDropItemCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPortalCageStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserPossessionAutoConvert",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PossessionType = table.Column<int>(type: "INTEGER", nullable: false),
                    PossessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ToPossessionType = table.Column<int>(type: "INTEGER", nullable: false),
                    ToPossessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ConvertDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPossessionAutoConvert", x => new { x.UserId, x.PossessionType, x.PossessionId });
                });

            migrationBuilder.CreateTable(
                name: "IUserPremiumItem",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PremiumItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPremiumItem", x => new { x.UserId, x.PremiumItemId });
                });

            migrationBuilder.CreateTable(
                name: "IUserProfile",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    MessageUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    FavoriteCostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    FavoriteCostumeIdUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserProfile", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserPvpDefenseDeck",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserDeckNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPvpDefenseDeck", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserPvpStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    StaminaMilliValue = table.Column<int>(type: "INTEGER", nullable: false),
                    StaminaUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestRewardReceivePvpSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestRewardReceivePvpWeeklyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    WinStreakCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WinStreakCountUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPvpStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserPvpWeeklyResult",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PvpWeeklyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    PvpSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalPoint = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalRank = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserPvpWeeklyResult", x => new { x.UserId, x.PvpWeeklyVersion });
                });

            migrationBuilder.CreateTable(
                name: "IUserQuest",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestStateType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBattleOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestStartDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    ClearCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyClearCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastClearDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    ShortestClearFrames = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserQuest", x => new { x.UserId, x.QuestId });
                });

            migrationBuilder.CreateTable(
                name: "IUserQuestAutoOrbit",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestType = table.Column<int>(type: "INTEGER", nullable: false),
                    ChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAutoOrbitCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ClearedAutoOrbitCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastClearDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserQuestAutoOrbit", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserQuestLimitContentStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    LimitContentQuestStatusType = table.Column<int>(type: "INTEGER", nullable: false),
                    EventQuestChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserQuestLimitContentStatus", x => new { x.UserId, x.QuestId });
                });

            migrationBuilder.CreateTable(
                name: "IUserQuestMission",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestMissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressValue = table.Column<int>(type: "INTEGER", nullable: false),
                    IsClear = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestClearDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserQuestMission", x => new { x.UserId, x.QuestId, x.QuestMissionId });
                });

            migrationBuilder.CreateTable(
                name: "IUserQuestReplayFlowRewardGroup",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestReplayFlowRewardGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiveDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserQuestReplayFlowRewardGroup", x => new { x.UserId, x.QuestReplayFlowRewardGroupId });
                });

            migrationBuilder.CreateTable(
                name: "IUserQuestSceneChoice",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestSceneChoiceGroupingId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestSceneChoiceEffectId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserQuestSceneChoice", x => new { x.UserId, x.QuestSceneChoiceGroupingId });
                });

            migrationBuilder.CreateTable(
                name: "IUserQuestSceneChoiceHistory",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestSceneChoiceEffectId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChoiceDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserQuestSceneChoiceHistory", x => new { x.UserId, x.QuestSceneChoiceEffectId });
                });

            migrationBuilder.CreateTable(
                name: "IUserSetting",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsNotifyPurchaseAlert = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserSetting", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserShopItem",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ShopItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    BoughtCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestBoughtCountChangedDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserShopItem", x => new { x.UserId, x.ShopItemId });
                });

            migrationBuilder.CreateTable(
                name: "IUserShopReplaceable",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    LineupUpdateCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestLineupUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserShopReplaceable", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserShopReplaceableLineup",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ShopItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserShopReplaceableLineup", x => new { x.UserId, x.SlotNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserSideStoryQuest",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SideStoryQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadSideStoryQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    SideStoryQuestStateType = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserSideStoryQuest", x => new { x.UserId, x.SideStoryQuestId });
                });

            migrationBuilder.CreateTable(
                name: "IUserSideStoryQuestSceneProgressStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentSideStoryQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentSideStoryQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserSideStoryQuestSceneProgressStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserStatus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Exp = table.Column<int>(type: "INTEGER", nullable: false),
                    StaminaMilliValue = table.Column<int>(type: "INTEGER", nullable: false),
                    StaminaUpdateDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserStatus", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "IUserThought",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserThoughtUuid = table.Column<string>(type: "TEXT", nullable: false),
                    ThoughtId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserThought", x => new { x.UserId, x.UserThoughtUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserTripleDeck",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    DeckType = table.Column<int>(type: "INTEGER", nullable: false),
                    UserDeckNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DeckNumber01 = table.Column<int>(type: "INTEGER", nullable: false),
                    DeckNumber02 = table.Column<int>(type: "INTEGER", nullable: false),
                    DeckNumber03 = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserTripleDeck", x => new { x.UserId, x.DeckType, x.UserDeckNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserTutorialProgress",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    TutorialType = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressPhase = table.Column<int>(type: "INTEGER", nullable: false),
                    ChoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserTutorialProgress", x => new { x.UserId, x.TutorialType });
                });

            migrationBuilder.CreateTable(
                name: "IUserWeapon",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserWeaponUuid = table.Column<string>(type: "TEXT", nullable: false),
                    WeaponId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Exp = table.Column<int>(type: "INTEGER", nullable: false),
                    LimitBreakCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsProtected = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserWeapon", x => new { x.UserId, x.UserWeaponUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserWeaponAbility",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserWeaponUuid = table.Column<string>(type: "TEXT", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserWeaponAbility", x => new { x.UserId, x.UserWeaponUuid, x.SlotNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserWeaponAwaken",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserWeaponUuid = table.Column<string>(type: "TEXT", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserWeaponAwaken", x => new { x.UserId, x.UserWeaponUuid });
                });

            migrationBuilder.CreateTable(
                name: "IUserWeaponNote",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    WeaponId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxLimitBreakCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstAcquisitionDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserWeaponNote", x => new { x.UserId, x.WeaponId });
                });

            migrationBuilder.CreateTable(
                name: "IUserWeaponSkill",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserWeaponUuid = table.Column<string>(type: "TEXT", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserWeaponSkill", x => new { x.UserId, x.UserWeaponUuid, x.SlotNumber });
                });

            migrationBuilder.CreateTable(
                name: "IUserWeaponStory",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    WeaponId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReleasedMaxStoryIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserWeaponStory", x => new { x.UserId, x.WeaponId });
                });

            migrationBuilder.CreateTable(
                name: "IUserWebviewPanelMission",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    WebviewPanelMissionPageId = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardReceiveDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IUserWebviewPanelMission", x => new { x.UserId, x.WebviewPanelMissionPageId });
                });

            migrationBuilder.CreateTable(
                name: "SBattleDetail",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BattleBinary = table.Column<string>(type: "TEXT", nullable: false),
                    ElapsedFrameCount = table.Column<long>(type: "INTEGER", nullable: false),
                    MaxDamage = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerCostumeActiveSkillUsedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerWeaponActiveSkillUsedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerCompanionSkillUsedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ComboCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ComboMaxDamage = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRecoverPoint = table.Column<long>(type: "INTEGER", nullable: false),
                    CostumeBattleInfoList = table.Column<string>(type: "TEXT", nullable: false),
                    UserPartyResultInfoList = table.Column<string>(type: "TEXT", nullable: false),
                    NpcPartyResultInfoList = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SBattleDetail", x => new { x.UserId, x.RecordedAt, x.QuestId });
                });

            migrationBuilder.CreateTable(
                name: "SBigHuntSession",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BattleBinary = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DeckNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DeckType = table.Column<int>(type: "INTEGER", nullable: false),
                    UserTripleDeckNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    BossKnockDownCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxComboCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalDamage = table.Column<long>(type: "INTEGER", nullable: false),
                    BattleReportCostume = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SBigHuntSession", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionKey = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionKey);
                });

            migrationBuilder.CreateTable(
                name: "SGachaBannerState",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GachaId = table.Column<int>(type: "INTEGER", nullable: false),
                    BoxNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DrawCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MedalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    StepNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LoopCount = table.Column<int>(type: "INTEGER", nullable: false),
                    BoxDrewCounts = table.Column<string>(type: "TEXT", nullable: false),
                    BoxRefillDatetime = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SGachaBannerState", x => new { x.UserId, x.GachaId });
                });

            migrationBuilder.CreateTable(
                name: "SGachaRewardState",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    LastRewardDrawDate = table.Column<long>(type: "INTEGER", nullable: false),
                    TodaysCurrentDrawCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyMaxCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SGachaRewardState", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SharedPrizeClaims",
                columns: table => new
                {
                    PrizeKey = table.Column<string>(type: "TEXT", nullable: false),
                    Claimed = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedPrizeClaims", x => x.PrizeKey);
                });

            migrationBuilder.CreateTable(
                name: "SQuestAutoOrbit",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Drops = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SQuestAutoOrbit", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SQuestSession",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserDeckNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentEventQuestChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQuestFlowType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBattleOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMenuPick = table.Column<bool>(type: "INTEGER", nullable: false),
                    SavedQuestFlowType = table.Column<int>(type: "INTEGER", nullable: false),
                    SavedPortalCageInProgress = table.Column<bool>(type: "INTEGER", nullable: false),
                    SavedSideStoryQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    SavedSideStoryQuestSceneId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SQuestSession", x => new { x.UserId, x.QuestId });
                });

            migrationBuilder.CreateTable(
                name: "SUser",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BackupToken = table.Column<string>(type: "TEXT", nullable: false),
                    ChargeMoneyThisMonth = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUser", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SUserDevice",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Uuid = table.Column<string>(type: "TEXT", nullable: false),
                    AdvertisingId = table.Column<string>(type: "TEXT", nullable: false),
                    IsTrackingEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IdentifierForVendor = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceToken = table.Column<string>(type: "TEXT", nullable: false),
                    MacAddress = table.Column<string>(type: "TEXT", nullable: false),
                    RegistrationId = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredAt = table.Column<long>(type: "INTEGER", nullable: false),
                    LastAuthAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUserDevice", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SUserGift",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserGiftUuid = table.Column<string>(type: "TEXT", nullable: false),
                    PossessionType = table.Column<int>(type: "INTEGER", nullable: false),
                    PossessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    GrantDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    DescriptionGiftTextId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentData = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ExpirationDatetime = table.Column<long>(type: "INTEGER", nullable: false),
                    ReceivedDatetime = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUserGift", x => new { x.UserId, x.UserGiftUuid });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IUser_PlayerId",
                table: "IUser",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ExpiresAt",
                table: "Sessions",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "IUser");

            migrationBuilder.DropTable(
                name: "IUserApple");

            migrationBuilder.DropTable(
                name: "IUserAutoSaleSettingDetail");

            migrationBuilder.DropTable(
                name: "IUserBeginnerCampaign");

            migrationBuilder.DropTable(
                name: "IUserBigHuntMaxScore");

            migrationBuilder.DropTable(
                name: "IUserBigHuntProgressStatus");

            migrationBuilder.DropTable(
                name: "IUserBigHuntScheduleMaxScore");

            migrationBuilder.DropTable(
                name: "IUserBigHuntStatus");

            migrationBuilder.DropTable(
                name: "IUserBigHuntWeeklyMaxScore");

            migrationBuilder.DropTable(
                name: "IUserBigHuntWeeklyStatus");

            migrationBuilder.DropTable(
                name: "IUserCageOrnamentReward");

            migrationBuilder.DropTable(
                name: "IUserCharacter");

            migrationBuilder.DropTable(
                name: "IUserCharacterBoard");

            migrationBuilder.DropTable(
                name: "IUserCharacterBoardAbility");

            migrationBuilder.DropTable(
                name: "IUserCharacterBoardCompleteReward");

            migrationBuilder.DropTable(
                name: "IUserCharacterBoardStatusUp");

            migrationBuilder.DropTable(
                name: "IUserCharacterCostumeLevelBonus");

            migrationBuilder.DropTable(
                name: "IUserCharacterRebirth");

            migrationBuilder.DropTable(
                name: "IUserCharacterViewerField");

            migrationBuilder.DropTable(
                name: "IUserComebackCampaign");

            migrationBuilder.DropTable(
                name: "IUserCompanion");

            migrationBuilder.DropTable(
                name: "IUserConsumableItem");

            migrationBuilder.DropTable(
                name: "IUserContentsStory");

            migrationBuilder.DropTable(
                name: "IUserCostume");

            migrationBuilder.DropTable(
                name: "IUserCostumeActiveSkill");

            migrationBuilder.DropTable(
                name: "IUserCostumeAwakenStatusUp");

            migrationBuilder.DropTable(
                name: "IUserCostumeLevelBonusReleaseStatus");

            migrationBuilder.DropTable(
                name: "IUserCostumeLotteryEffect");

            migrationBuilder.DropTable(
                name: "IUserCostumeLotteryEffectAbility");

            migrationBuilder.DropTable(
                name: "IUserCostumeLotteryEffectPending");

            migrationBuilder.DropTable(
                name: "IUserCostumeLotteryEffectStatusUp");

            migrationBuilder.DropTable(
                name: "IUserDeck");

            migrationBuilder.DropTable(
                name: "IUserDeckCharacter");

            migrationBuilder.DropTable(
                name: "IUserDeckCharacterDressupCostume");

            migrationBuilder.DropTable(
                name: "IUserDeckLimitContentDeletedCharacter");

            migrationBuilder.DropTable(
                name: "IUserDeckLimitContentRestricted");

            migrationBuilder.DropTable(
                name: "IUserDeckPartsGroup");

            migrationBuilder.DropTable(
                name: "IUserDeckSubWeaponGroup");

            migrationBuilder.DropTable(
                name: "IUserDeckTypeNote");

            migrationBuilder.DropTable(
                name: "IUserDokan");

            migrationBuilder.DropTable(
                name: "IUserEventQuestDailyGroupCompleteReward");

            migrationBuilder.DropTable(
                name: "IUserEventQuestGuerrillaFreeOpen");

            migrationBuilder.DropTable(
                name: "IUserEventQuestLabyrinthSeason");

            migrationBuilder.DropTable(
                name: "IUserEventQuestLabyrinthStage");

            migrationBuilder.DropTable(
                name: "IUserEventQuestProgressStatus");

            migrationBuilder.DropTable(
                name: "IUserEventQuestTowerAccumulationReward");

            migrationBuilder.DropTable(
                name: "IUserExplore");

            migrationBuilder.DropTable(
                name: "IUserExploreScore");

            migrationBuilder.DropTable(
                name: "IUserExtraQuestProgressStatus");

            migrationBuilder.DropTable(
                name: "IUserFacebook");

            migrationBuilder.DropTable(
                name: "IUserGem");

            migrationBuilder.DropTable(
                name: "IUserGimmick");

            migrationBuilder.DropTable(
                name: "IUserGimmickOrnamentProgress");

            migrationBuilder.DropTable(
                name: "IUserGimmickSequence");

            migrationBuilder.DropTable(
                name: "IUserGimmickUnlock");

            migrationBuilder.DropTable(
                name: "IUserImportantItem");

            migrationBuilder.DropTable(
                name: "IUserLimitedOpen");

            migrationBuilder.DropTable(
                name: "IUserLogin");

            migrationBuilder.DropTable(
                name: "IUserLoginBonus");

            migrationBuilder.DropTable(
                name: "IUserMainQuestFlowStatus");

            migrationBuilder.DropTable(
                name: "IUserMainQuestMainFlowStatus");

            migrationBuilder.DropTable(
                name: "IUserMainQuestProgressStatus");

            migrationBuilder.DropTable(
                name: "IUserMainQuestReplayFlowStatus");

            migrationBuilder.DropTable(
                name: "IUserMainQuestSeasonRoute");

            migrationBuilder.DropTable(
                name: "IUserMaterial");

            migrationBuilder.DropTable(
                name: "IUserMission");

            migrationBuilder.DropTable(
                name: "IUserMissionCompletionProgress");

            migrationBuilder.DropTable(
                name: "IUserMissionPassPoint");

            migrationBuilder.DropTable(
                name: "IUserMovie");

            migrationBuilder.DropTable(
                name: "IUserNaviCutIn");

            migrationBuilder.DropTable(
                name: "IUserOmikuji");

            migrationBuilder.DropTable(
                name: "IUserParts");

            migrationBuilder.DropTable(
                name: "IUserPartsGroupNote");

            migrationBuilder.DropTable(
                name: "IUserPartsPreset");

            migrationBuilder.DropTable(
                name: "IUserPartsPresetTag");

            migrationBuilder.DropTable(
                name: "IUserPartsStatusSub");

            migrationBuilder.DropTable(
                name: "IUserPortalCageStatus");

            migrationBuilder.DropTable(
                name: "IUserPossessionAutoConvert");

            migrationBuilder.DropTable(
                name: "IUserPremiumItem");

            migrationBuilder.DropTable(
                name: "IUserProfile");

            migrationBuilder.DropTable(
                name: "IUserPvpDefenseDeck");

            migrationBuilder.DropTable(
                name: "IUserPvpStatus");

            migrationBuilder.DropTable(
                name: "IUserPvpWeeklyResult");

            migrationBuilder.DropTable(
                name: "IUserQuest");

            migrationBuilder.DropTable(
                name: "IUserQuestAutoOrbit");

            migrationBuilder.DropTable(
                name: "IUserQuestLimitContentStatus");

            migrationBuilder.DropTable(
                name: "IUserQuestMission");

            migrationBuilder.DropTable(
                name: "IUserQuestReplayFlowRewardGroup");

            migrationBuilder.DropTable(
                name: "IUserQuestSceneChoice");

            migrationBuilder.DropTable(
                name: "IUserQuestSceneChoiceHistory");

            migrationBuilder.DropTable(
                name: "IUserSetting");

            migrationBuilder.DropTable(
                name: "IUserShopItem");

            migrationBuilder.DropTable(
                name: "IUserShopReplaceable");

            migrationBuilder.DropTable(
                name: "IUserShopReplaceableLineup");

            migrationBuilder.DropTable(
                name: "IUserSideStoryQuest");

            migrationBuilder.DropTable(
                name: "IUserSideStoryQuestSceneProgressStatus");

            migrationBuilder.DropTable(
                name: "IUserStatus");

            migrationBuilder.DropTable(
                name: "IUserThought");

            migrationBuilder.DropTable(
                name: "IUserTripleDeck");

            migrationBuilder.DropTable(
                name: "IUserTutorialProgress");

            migrationBuilder.DropTable(
                name: "IUserWeapon");

            migrationBuilder.DropTable(
                name: "IUserWeaponAbility");

            migrationBuilder.DropTable(
                name: "IUserWeaponAwaken");

            migrationBuilder.DropTable(
                name: "IUserWeaponNote");

            migrationBuilder.DropTable(
                name: "IUserWeaponSkill");

            migrationBuilder.DropTable(
                name: "IUserWeaponStory");

            migrationBuilder.DropTable(
                name: "IUserWebviewPanelMission");

            migrationBuilder.DropTable(
                name: "SBattleDetail");

            migrationBuilder.DropTable(
                name: "SBigHuntSession");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "SGachaBannerState");

            migrationBuilder.DropTable(
                name: "SGachaRewardState");

            migrationBuilder.DropTable(
                name: "SharedPrizeClaims");

            migrationBuilder.DropTable(
                name: "SQuestAutoOrbit");

            migrationBuilder.DropTable(
                name: "SQuestSession");

            migrationBuilder.DropTable(
                name: "SUser");

            migrationBuilder.DropTable(
                name: "SUserDevice");

            migrationBuilder.DropTable(
                name: "SUserGift");
        }
    }
}
