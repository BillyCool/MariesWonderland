// TODO: Unreviewed tests, commented out until they are reviewed and fixed up.
// using MariesWonderland.Data;
// using MariesWonderland.Models.Entities;
// using MariesWonderland.Models.Type;
// using MariesWonderland.Proto.Labyrinth;
// using MariesWonderland.Tests.Infrastructure;
// using LabyrinthServiceImpl = MariesWonderland.Services.LabyrinthService;
// 
// namespace MariesWonderland.Tests.Services;
// 
// public class LabyrinthServiceTests
// {
//     private const long UserId = 1L;
//     private const int ChapterId = 100;
//     private const int StageOrder = 1;
// 
//     [Fact]
//     public async Task UpdateSeasonData_ReturnsResolvedMilestonesAndRewards()
//     {
//         DarkMasterMemoryDatabase masterDb = CreateMasterDb();
//         LabyrinthServiceImpl service = CreateService(masterDb, new DarkUserMemoryDatabase());
// 
//         UpdateSeasonDataResponse response = await service.UpdateSeasonData(
//             new UpdateSeasonDataRequest { EventQuestChapterId = ChapterId },
//             FakeServerCallContext.For(UserId));
// 
//         Assert.Equal(3, response.SeasonResult.Count);
//         Assert.Collection(
//             response.SeasonResult,
//             first =>
//             {
//                 Assert.Equal(ChapterId, first.EventQuestChapterId);
//                 Assert.Equal(1001, first.HeadQuestId);
//                 Assert.Equal(1, first.HeadStageOrder);
//                 Assert.Collection(first.SeasonReward, reward => Assert.Equal("5:501:1", RewardKey(reward)));
//             },
//             second =>
//             {
//                 Assert.Equal(1002, second.HeadQuestId);
//                 Assert.Equal(2, second.HeadStageOrder);
//                 Assert.Collection(second.SeasonReward, reward => Assert.Equal("5:502:2", RewardKey(reward)));
//             },
//             third =>
//             {
//                 Assert.Equal(1003, third.HeadQuestId);
//                 Assert.Equal(2, third.HeadStageOrder);
//                 Assert.Collection(third.SeasonReward, reward => Assert.Equal("5:502:2", RewardKey(reward)));
//             });
//     }
// 
//     [Fact]
//     public async Task ReceiveStageClearReward_GrantsRewardOnceAndPersistsState()
//     {
//         DarkMasterMemoryDatabase masterDb = CreateMasterDb();
//         DarkUserMemoryDatabase userDb = new();
//         LabyrinthServiceImpl service = CreateService(masterDb, userDb);
// 
//         await service.ReceiveStageClearReward(
//             new ReceiveStageClearRewardRequest { EventQuestChapterId = ChapterId, StageOrder = StageOrder },
//             FakeServerCallContext.For(UserId));
//         await service.ReceiveStageClearReward(
//             new ReceiveStageClearRewardRequest { EventQuestChapterId = ChapterId, StageOrder = StageOrder },
//             FakeServerCallContext.For(UserId));
// 
//         EntityIUserEventQuestLabyrinthStage stage = Assert.Single(userDb.EntityIUserEventQuestLabyrinthStage);
//         Assert.True(stage.IsReceivedStageClearReward);
//         Assert.True(stage.LatestVersion > 0);
// 
//         EntityIUserMaterial reward = Assert.Single(userDb.EntityIUserMaterial);
//         Assert.Equal(601, reward.MaterialId);
//         Assert.Equal(3, reward.Count);
//     }
// 
//     [Fact]
//     public async Task ReceiveStageAccumulationReward_GrantsOnlyNewThresholdRewards()
//     {
//         DarkMasterMemoryDatabase masterDb = CreateMasterDb();
//         DarkUserMemoryDatabase userDb = new();
//         LabyrinthServiceImpl service = CreateService(masterDb, userDb);
// 
//         await service.ReceiveStageAccumulationReward(
//             new ReceiveStageAccumulationRewardRequest
//             {
//                 EventQuestChapterId = ChapterId,
//                 StageOrder = StageOrder,
//                 QuestMissionClearCount = 3
//             },
//             FakeServerCallContext.For(UserId));
//         await service.ReceiveStageAccumulationReward(
//             new ReceiveStageAccumulationRewardRequest
//             {
//                 EventQuestChapterId = ChapterId,
//                 StageOrder = StageOrder,
//                 QuestMissionClearCount = 5
//             },
//             FakeServerCallContext.For(UserId));
//         await service.ReceiveStageAccumulationReward(
//             new ReceiveStageAccumulationRewardRequest
//             {
//                 EventQuestChapterId = ChapterId,
//                 StageOrder = StageOrder,
//                 QuestMissionClearCount = 5
//             },
//             FakeServerCallContext.For(UserId));
// 
//         EntityIUserEventQuestLabyrinthStage stage = Assert.Single(userDb.EntityIUserEventQuestLabyrinthStage);
//         Assert.Equal(5, stage.AccumulationRewardReceivedQuestMissionCount);
// 
//         Assert.Equal(
//             ["602:1", "603:2"],
//             userDb.EntityIUserMaterial.OrderBy(m => m.MaterialId).Select(m => $"{m.MaterialId}:{m.Count}").ToArray());
//     }
// 
//     private static LabyrinthServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
//     {
//         UserDataStore store = new(masterDb);
//         store.Set(UserId, userDb);
//         return new LabyrinthServiceImpl(store, masterDb);
//     }
// 
//     private static DarkMasterMemoryDatabase CreateMasterDb()
//     {
//         DarkMasterMemoryDatabase masterDb = new();
//         masterDb.EntityMEventQuestLabyrinthSeason.AddRange(
//         [
//             new EntityMEventQuestLabyrinthSeason { EventQuestChapterId = ChapterId, SeasonNumber = 1, SeasonRewardGroupId = 700 },
//             new EntityMEventQuestLabyrinthSeason { EventQuestChapterId = ChapterId, SeasonNumber = 2, SeasonRewardGroupId = 700 }
//         ]);
//         masterDb.EntityMEventQuestLabyrinthSeasonRewardGroup.AddRange(
//         [
//             new EntityMEventQuestLabyrinthSeasonRewardGroup { EventQuestLabyrinthSeasonRewardGroupId = 700, HeadQuestId = 1001, EventQuestLabyrinthRewardGroupId = 701 },
//             new EntityMEventQuestLabyrinthSeasonRewardGroup { EventQuestLabyrinthSeasonRewardGroupId = 700, HeadQuestId = 1002, EventQuestLabyrinthRewardGroupId = 702 },
//             new EntityMEventQuestLabyrinthSeasonRewardGroup { EventQuestLabyrinthSeasonRewardGroupId = 700, HeadQuestId = 1003, EventQuestLabyrinthRewardGroupId = 702 }
//         ]);
//         masterDb.EntityMEventQuestLabyrinthStage.Add(new EntityMEventQuestLabyrinthStage
//         {
//             EventQuestChapterId = ChapterId,
//             StageOrder = StageOrder,
//             StageClearRewardGroupId = 703,
//             StageAccumulationRewardGroupId = 704
//         });
//         masterDb.EntityMEventQuestLabyrinthStageAccumulationRewardGroup.AddRange(
//         [
//             new EntityMEventQuestLabyrinthStageAccumulationRewardGroup
//             {
//                 EventQuestLabyrinthStageAccumulationRewardGroupId = 704,
//                 QuestMissionClearCount = 3,
//                 EventQuestLabyrinthRewardGroupId = 705
//             },
//             new EntityMEventQuestLabyrinthStageAccumulationRewardGroup
//             {
//                 EventQuestLabyrinthStageAccumulationRewardGroupId = 704,
//                 QuestMissionClearCount = 5,
//                 EventQuestLabyrinthRewardGroupId = 706
//             }
//         ]);
//         masterDb.EntityMEventQuestLabyrinthRewardGroup.AddRange(
//         [
//             new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 701, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 501, Count = 1 },
//             new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 702, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 502, Count = 2 },
//             new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 703, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 601, Count = 3 },
//             new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 705, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 602, Count = 1 },
//             new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 706, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 603, Count = 2 }
//         ]);
//         return masterDb;
//     }
// 
//     private static string RewardKey(LabyrinthReward reward)
//         => $"{reward.PossessionType}:{reward.PossessionId}:{reward.Count}";
// }
