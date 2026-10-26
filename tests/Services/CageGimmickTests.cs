using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gimmick;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;
using GimmickServiceImpl = MariesWonderland.Services.GimmickService;

namespace MariesWonderland.Tests.Services;

/// <summary>
/// Cage gimmicks on map replays: the rows the client needs before it shows a gimmick, the release condition that holds
/// back Lost Archives, and what fickle black birds, Lost Items and Lost Archives give.
/// </summary>
public class CageGimmickTests
{
    private const long UserId = 1L;
    private const int GoldId = 1;
    private const long MinuteMs = 60_000;
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly long NowMs = Now.ToUnixTimeMilliseconds();

    private readonly DarkMasterMemoryDatabase _masterDb = new();
    private readonly DarkUserMemoryDatabase _userDb = new() { UserId = UserId };
    private readonly FakeTimeProvider _time = new(Now);

    private GimmickServiceImpl Service()
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, _userDb);
        return new GimmickServiceImpl(store, _masterDb, new GameConfig { ConsumableItemIdForGold = GoldId }, TestServices.Missions(_masterDb), _time);
    }

    private void Schedule(int scheduleId, int firstSequenceId, long start = 0, long end = long.MaxValue)
        => _masterDb.EntityMGimmickSequenceSchedule.Add(new EntityMGimmickSequenceSchedule
        {
            GimmickSequenceScheduleId = scheduleId,
            FirstGimmickSequenceId = firstSequenceId,
            StartDatetime = start == 0 ? NowMs - 1000 : start,
            EndDatetime = end
        });

    /// <summary>A sequence holding one gimmick of the given type with <paramref name="ornaments"/> ornaments.</summary>
    private void Gimmick(int sequenceId, int gimmickId, GimmickType type, int ornaments = 1, int releaseConditionId = 0)
    {
        int groupId = sequenceId * 10;
        if (!_masterDb.EntityMGimmickSequence.Any(s => s.GimmickSequenceId == sequenceId))
        {
            _masterDb.EntityMGimmickSequence.Add(new EntityMGimmickSequence { GimmickSequenceId = sequenceId, GimmickGroupId = groupId });
        }

        _masterDb.EntityMGimmickGroup.Add(new EntityMGimmickGroup { GimmickGroupId = groupId, GroupIndex = _masterDb.EntityMGimmickGroup.Count(g => g.GimmickGroupId == groupId) + 1, GimmickId = gimmickId });
        _masterDb.EntityMGimmick.Add(new EntityMGimmick { GimmickId = gimmickId, GimmickType = type, GimmickOrnamentGroupId = gimmickId, ReleaseEvaluateConditionId = releaseConditionId });
        for (int i = 1; i <= ornaments; i++)
        {
            _masterDb.EntityMGimmickOrnament.Add(new EntityMGimmickOrnament { GimmickOrnamentGroupId = gimmickId, GimmickOrnamentIndex = i });
        }
    }

    private void Interval(int gimmickId, int initial, int max, int minutes)
        => _masterDb.EntityMGimmickInterval.Add(new EntityMGimmickInterval { GimmickId = gimmickId, InitialValue = initial, MaxValue = max, IntervalValue = minutes });

    private EntityIUserGimmickOrnamentProgress Ornament(int gimmickId, int index = 1)
        => _userDb.EntityIUserGimmickOrnamentProgress.Single(o => o.GimmickId == gimmickId && o.GimmickOrnamentIndex == index);

    private long Gold() => _userDb.EntityIUserConsumableItem.Where(c => c.ConsumableItemId == GoldId).Sum(c => (long)c.Count);

    private static UpdateGimmickProgressRequest Pickup(int scheduleId, int sequenceId, int gimmickId, int bits, int ornamentIndex = 1)
        => new() { GimmickSequenceScheduleId = scheduleId, GimmickSequenceId = sequenceId, GimmickId = gimmickId, GimmickOrnamentIndex = ornamentIndex, ProgressValueBit = bits };

    [Fact]
    public async Task InitSequenceSchedule_WritesTheRowsEveryGimmickNeedsToShow()
    {
        Schedule(1, 10);
        Gimmick(10, 100, GimmickType.CAGE_TREASURE_HUNT);
        Gimmick(10, 200, GimmickType.CAGE_INTERVAL_DROP_ITEM, ornaments: 2);
        Interval(200, initial: 2, max: 5, minutes: 60);

        await Service().InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Single(_userDb.EntityIUserGimmickSequence);
        Assert.Equal([100, 200], _userDb.EntityIUserGimmick.Select(g => g.GimmickId).Order());
        Assert.All(_userDb.EntityIUserGimmick, g => Assert.Equal((NowMs, false), (g.StartDatetime, g.IsGimmickCleared)));
        Assert.Equal(3, _userDb.EntityIUserGimmickOrnamentProgress.Count);
        Assert.All(_userDb.EntityIUserGimmickOrnamentProgress, o => Assert.Equal(0, o.ProgressValueBit));
        Assert.Equal(NowMs, Ornament(100).BaseDatetime);
        // Lost Items start with their initial pile: 2 items of 60 minutes.
        Assert.Equal(NowMs - 120 * MinuteMs, Ornament(200, 2).BaseDatetime);

        // Running it again (every cage load) changes nothing.
        await Service().InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));
        Assert.Equal(2, _userDb.EntityIUserGimmick.Count);
        Assert.Equal(3, _userDb.EntityIUserGimmickOrnamentProgress.Count);
    }

    [Fact]
    public async Task ALostArchive_WaitsUntilTheArchiveItNeedsHasBeenRead()
    {
        // Archive 200 appears only once archive 100 (schedule 1) has a bit set: OTHER_GIMMICK_BIT_COUNT >= 1.
        _masterDb.EntityMEvaluateCondition.Add(new EntityMEvaluateCondition
        {
            EvaluateConditionId = 6001,
            EvaluateConditionFunctionType = EvaluateConditionFunctionType.OTHER_GIMMICK_BIT_COUNT,
            EvaluateConditionEvaluateType = EvaluateConditionEvaluateType.GE,
            EvaluateConditionValueGroupId = 6001
        });
        long[] values = [1, 10, 100, 1, 1];
        for (int i = 0; i < values.Length; i++)
        {
            _masterDb.EntityMEvaluateConditionValueGroup.Add(new EntityMEvaluateConditionValueGroup { EvaluateConditionValueGroupId = 6001, GroupIndex = i + 1, Value = values[i] });
        }

        Schedule(1, 10);
        Schedule(2, 20);
        Gimmick(10, 100, GimmickType.CAGE_MEMORY);
        Gimmick(20, 200, GimmickType.CAGE_MEMORY, releaseConditionId: 6001);
        GimmickServiceImpl service = Service();

        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));
        Assert.Equal([100], _userDb.EntityIUserGimmick.Select(g => g.GimmickId));

        await service.UpdateGimmickProgress(Pickup(1, 10, 100, bits: 1), FakeServerCallContext.For(UserId));
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Equal([100, 200], _userDb.EntityIUserGimmick.Select(g => g.GimmickId).Order());
    }

    [Fact]
    public async Task TheNextHiddenStory_GetsItsRows_AndStaysAfterTheNextCageLoad()
    {
        Schedule(1, 10);
        Gimmick(10, 100, GimmickType.REPORT);
        Gimmick(11, 110, GimmickType.REPORT);
        GimmickServiceImpl service = Service();
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        await service.UpdateSequence(new UpdateSequenceRequest { GimmickSequenceScheduleId = 1, GimmickSequenceId = 11 }, FakeServerCallContext.For(UserId));
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Contains(_userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceId == 11);
        Assert.Contains(_userDb.EntityIUserGimmick, g => g.GimmickId == 110 && g.GimmickSequenceId == 11);
        Assert.Single(_userDb.EntityIUserGimmickOrnamentProgress, o => o.GimmickId == 110);
    }

    [Fact]
    public async Task AFickleBirdsWindowEnding_DropsItsUnfinishedRows()
    {
        Schedule(1, 10, start: NowMs - 10_000, end: NowMs + 10_000);
        Gimmick(10, 100, GimmickType.CAGE_TREASURE_HUNT);
        GimmickServiceImpl service = Service();
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));
        Assert.Single(_userDb.EntityIUserGimmick);

        _time.Advance(TimeSpan.FromMinutes(1));
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Empty(_userDb.EntityIUserGimmickSequence);
        Assert.Empty(_userDb.EntityIUserGimmick);
        Assert.Empty(_userDb.EntityIUserGimmickOrnamentProgress);
    }

    [Fact]
    public async Task AFickleBird_GivesItsRewardOnce()
    {
        Schedule(1, 10);
        Gimmick(10, 100, GimmickType.CAGE_TREASURE_HUNT);
        GimmickServiceImpl service = Service();
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        UpdateGimmickProgressResponse first = await service.UpdateGimmickProgress(Pickup(1, 10, 100, bits: 1), FakeServerCallContext.For(UserId));
        UpdateGimmickProgressResponse again = await service.UpdateGimmickProgress(Pickup(1, 10, 100, bits: 1), FakeServerCallContext.For(UserId));

        GimmickReward reward = Assert.Single(first.GimmickOrnamentReward);
        Assert.Equal(((int)PossessionType.CONSUMABLE_ITEM, GoldId, 1000), (reward.PossessionType, reward.PossessionId, reward.Count));
        Assert.Empty(again.GimmickOrnamentReward);
        Assert.Equal(1000, Gold());
        Assert.False(_userDb.EntityIUserGimmick.Single().IsGimmickCleared);
    }

    [Fact]
    public async Task LostItems_GiveOneRollPerPiledItem_AndStartPilingAgain()
    {
        Schedule(1, 10);
        Gimmick(10, 200, GimmickType.CAGE_INTERVAL_DROP_ITEM);
        Interval(200, initial: 0, max: 5, minutes: 1440);
        GimmickServiceImpl service = Service();
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));
        Ornament(200).BaseDatetime = NowMs - 3 * 1440 * MinuteMs - 5 * MinuteMs;

        UpdateGimmickProgressResponse pickup = await service.UpdateGimmickProgress(Pickup(1, 10, 200, bits: 1), FakeServerCallContext.For(UserId));
        UpdateGimmickProgressResponse empty = await service.UpdateGimmickProgress(Pickup(1, 10, 200, bits: 1), FakeServerCallContext.For(UserId));

        Assert.Equal(3000, Assert.Single(pickup.GimmickOrnamentReward).Count);
        Assert.Empty(empty.GimmickOrnamentReward);
        Assert.Equal(3000, Gold());
        Assert.Equal(NowMs, Ornament(200).BaseDatetime);
    }

    [Fact]
    public async Task LostItems_StopPilingAtTheirMaximum()
    {
        Schedule(1, 10);
        Gimmick(10, 200, GimmickType.CAGE_INTERVAL_DROP_ITEM);
        Interval(200, initial: 0, max: 5, minutes: 1440);
        GimmickServiceImpl service = Service();
        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));
        Ornament(200).BaseDatetime = NowMs - 30L * 1440 * MinuteMs;

        UpdateGimmickProgressResponse pickup = await service.UpdateGimmickProgress(Pickup(1, 10, 200, bits: 1), FakeServerCallContext.For(UserId));

        Assert.Equal(5000, Assert.Single(pickup.GimmickOrnamentReward).Count);
    }

    [Fact]
    public async Task ALostArchive_OpensAgainOnARevisit_ButIsGrantedOnce()
    {
        Schedule(1, 10);
        Gimmick(10, 100, GimmickType.CAGE_MEMORY);
        _masterDb.EntityMGimmickOrnament.Single().ChapterId = 80;
        _masterDb.EntityMMainQuestChapter.Add(new EntityMMainQuestChapter { MainQuestChapterId = 80, MainQuestRouteId = 90 });
        _masterDb.EntityMMainQuestRoute.Add(new EntityMMainQuestRoute { MainQuestRouteId = 90, MainQuestSeasonId = 1 });
        _masterDb.EntityMCageMemory.Add(new EntityMCageMemory { CageMemoryId = 5000, MainQuestSeasonId = 1 });
        _masterDb.EntityMImportantItem.Add(new EntityMImportantItem { ImportantItemId = 6000, CageMemoryId = 5000, ImportantItemType = 4 });
        GimmickServiceImpl service = Service();

        await service.UpdateGimmickProgress(Pickup(1, 10, 100, bits: 1), FakeServerCallContext.For(UserId));
        UpdateGimmickProgressResponse revisit = await service.UpdateGimmickProgress(Pickup(1, 10, 100, bits: 1), FakeServerCallContext.For(UserId));

        Assert.Equal(6000, Assert.Single(revisit.GimmickOrnamentReward).PossessionId);
        Assert.Equal(1, Assert.Single(_userDb.EntityIUserImportantItem).Count);
    }
}
