using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Mission;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using MissionServiceImpl = MariesWonderland.Services.MissionService;
using RewardServiceImpl = MariesWonderland.Services.RewardService;

namespace MariesWonderland.Tests.Missions;

/// <summary>
/// Mission pass: points from pass missions, free rewards for every reached level, premium rewards with the pass's
/// premium item (earlier levels included), and leftovers of ended passes.
/// </summary>
public class MissionPassTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const int FirstPass = 10001;
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);
    private static long NowMs => Now.ToUnixTimeMilliseconds();

    private (UserDataStore Store, DarkUserMemoryDatabase UserDb, long UserId) CreateUser()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("mission-pass-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        return (store, userDb, userId);
    }

    private int RewardCount(int passId, int upToLevel, bool isPremium)
    {
        int groupId = MasterDb.EntityMMissionPass.Find(passId)!.MissionPassRewardGroupId;
        return MasterDb.EntityMMissionPassRewardGroup.Count(r => r.MissionPassRewardGroupId == groupId && r.Level <= upToLevel && r.IsPremium == isPremium);
    }

    private static EntityIUserMissionPassPoint Progress(DarkUserMemoryDatabase userDb, int passId)
        => userDb.EntityIUserMissionPassPoint.Single(p => p.MissionPassId == passId);

    [Fact]
    public void RealSave_ClaimedLevelsNeverPassTheReachedLevel()
    {
        DarkUserMemoryDatabase save = new UserDataSeeder(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = "Data/UserData" } })).LoadFromFiles();
        Assert.NotEmpty(save.EntityIUserMissionPassPoint);

        Assert.All(save.EntityIUserMissionPassPoint, p =>
        {
            int reached = MissionPassHelper.ReachedLevel(MasterDb, MasterDb.EntityMMissionPass.Find(p.MissionPassId)!, p.Point);
            Assert.Equal(15, reached);
            Assert.True(p.NoPremiumRewardReceivedLevel <= reached && p.PremiumRewardReceivedLevel <= reached, $"pass {p.MissionPassId}");
        });
    }

    [Fact]
    public void Points_AddUpOnThePassTheyName()
    {
        (_, DarkUserMemoryDatabase userDb, _) = CreateUser();

        PossessionHelper.Apply(userDb, PossessionType.MISSION_PASS_POINT, FirstPass, 80, MasterDb, NowMs);
        PossessionHelper.Apply(userDb, PossessionType.MISSION_PASS_POINT, FirstPass, 80, MasterDb, NowMs);

        Assert.Equal(160, Progress(userDb, FirstPass).Point);
    }

    [Fact]
    public void Claim_GrantsFreeLevels_ThenPremiumOnesOnceThePassIsBought()
    {
        (_, DarkUserMemoryDatabase userDb, _) = CreateUser();
        PossessionHelper.Apply(userDb, PossessionType.MISSION_PASS_POINT, FirstPass, 350, MasterDb, NowMs);

        List<MissionRewardGrant> free = MissionPassHelper.Claim(userDb, MasterDb, FirstPass, NowMs);
        Assert.Equal(RewardCount(FirstPass, 3, isPremium: false), free.Count);
        Assert.Equal(3, Progress(userDb, FirstPass).NoPremiumRewardReceivedLevel);
        Assert.Equal(0, Progress(userDb, FirstPass).PremiumRewardReceivedLevel);
        Assert.Empty(MissionPassHelper.Claim(userDb, MasterDb, FirstPass, NowMs));

        // Buying the premium pass pays out the premium rewards of the levels already reached.
        PossessionHelper.Apply(userDb, PossessionType.PREMIUM_ITEM, MasterDb.EntityMMissionPass.Find(FirstPass)!.PremiumItemId, 1, MasterDb, NowMs);
        List<MissionRewardGrant> premium = MissionPassHelper.Claim(userDb, MasterDb, FirstPass, NowMs);
        Assert.Equal(RewardCount(FirstPass, 3, isPremium: true), premium.Count);
        Assert.Equal(3, Progress(userDb, FirstPass).PremiumRewardReceivedLevel);
    }

    [Fact]
    public async Task ReceiveMissionPassRewards_ListsWhatItGranted()
    {
        (UserDataStore store, DarkUserMemoryDatabase userDb, long userId) = CreateUser();
        PossessionHelper.Apply(userDb, PossessionType.MISSION_PASS_POINT, FirstPass, 1500, MasterDb, NowMs);
        MissionServiceImpl service = new(store, MasterDb, new MissionTracker(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Empty)), new FakeTimeProvider(Now));

        ReceiveMissionPassRewardsResponse response = await service.ReceiveMissionPassRewards(new ReceiveMissionPassRewardsRequest { MissionPassId = FirstPass }, ContextFor(userId));

        Assert.Equal(RewardCount(FirstPass, 15, isPremium: false), response.ReceivedPossession.Count);
        Assert.Equal(15, Progress(userDb, FirstPass).NoPremiumRewardReceivedLevel);
    }

    [Fact]
    public void ClaimRemaining_TakesTheLatestEndedPassFirst()
    {
        (_, DarkUserMemoryDatabase userDb, _) = CreateUser();
        PossessionHelper.Apply(userDb, PossessionType.MISSION_PASS_POINT, 10001, 200, MasterDb, NowMs);
        PossessionHelper.Apply(userDb, PossessionType.MISSION_PASS_POINT, 10003, 200, MasterDb, NowMs);

        Assert.Equal(10003, MissionPassHelper.ClaimRemaining(userDb, MasterDb, NowMs));
        Assert.Equal(10001, MissionPassHelper.ClaimRemaining(userDb, MasterDb, NowMs));
        Assert.Equal(0, MissionPassHelper.ClaimRemaining(userDb, MasterDb, NowMs));
        Assert.Equal(2, Progress(userDb, 10003).NoPremiumRewardReceivedLevel);
    }

    [Fact]
    public async Task ReceiveMissionPassRemainingReward_ReturnsThePassItClaimed()
    {
        (UserDataStore store, DarkUserMemoryDatabase userDb, long userId) = CreateUser();
        PossessionHelper.Apply(userDb, PossessionType.MISSION_PASS_POINT, 10003, 300, MasterDb, NowMs);
        RewardServiceImpl service = new(store, MasterDb, new FakeTimeProvider(Now));

        Proto.Reward.ReceiveMissionPassRemainingRewardResponse first = await service.ReceiveMissionPassRemainingReward(new Google.Protobuf.WellKnownTypes.Empty(), ContextFor(userId));
        Proto.Reward.ReceiveMissionPassRemainingRewardResponse second = await service.ReceiveMissionPassRemainingReward(new Google.Protobuf.WellKnownTypes.Empty(), ContextFor(userId));

        Assert.Equal(10003, first.RewardReceivedMissionPassId);
        Assert.Equal(0, second.RewardReceivedMissionPassId);
        Assert.Equal(3, Progress(userDb, 10003).NoPremiumRewardReceivedLevel);
    }
}
