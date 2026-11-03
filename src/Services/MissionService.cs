using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Missions;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Mission;

namespace MariesWonderland.Services;

public class MissionService(UserDataStore store, DarkMasterMemoryDatabase masterDb, MissionTracker missions, TimeProvider timeProvider)
    : MariesWonderland.Proto.Mission.MissionService.MissionServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Takes the values the client measures itself and sends later (when the mission list refreshes): Cage values
    /// (metres walked, Mama taps) and magick users crossed out since the last report, and the last concert's tap count.
    /// </summary>
    public override Task<UpdateMissionProgressResponse> UpdateMissionProgress(UpdateMissionProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        if (request.CageMeasurableValues is { } cage)
        {
            _missions.RecordCageValues(userDb, cage.RunningDistanceMeters, cage.MamaTappedCount, nowMs);
        }

        int defeatedWizards = request.PictureBookMeasurableValues?.DefeatWizardCount ?? 0;
        if (defeatedWizards > 0)
        {
            _missions.Record(userDb, MissionClearConditionType.DEFEAT_WIZARD_COUNT, defeatedWizards, nowMs);
        }

        RhythmInteractionMeasurableValues? rhythm = request.PictureBookMeasurableValues?.RhythmInteractionMeasurableValues;
        if (rhythm != null && rhythm.LiveTypeId > 0 && rhythm.TapCount > 0)
        {
            _missions.RecordRhythmTaps(userDb, rhythm.LiveTypeId, rhythm.TapCount, nowMs);
        }

        return Task.FromResult(new UpdateMissionProgressResponse());
    }

    /// <summary>
    /// Sends the rewards of the given cleared missions to the gift box and marks the missions received. Rewards of
    /// a mission whose term has ended come back as expired instead of being sent.
    /// </summary>
    public override Task<ReceiveMissionRewardsResponse> ReceiveMissionRewardsById(ReceiveMissionRewardsByIdRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        (List<MissionRewardGrant> received, List<MissionRewardGrant> expired) = _missions.ReceiveRewards(userDb, request.MissionId, nowMs);
        // Rewards can raise levels and the like, and a received mission can unlock others.
        _missions.Refresh(userDb, nowMs);

        ReceiveMissionRewardsResponse response = new();
        response.ReceivedPossession.AddRange(received.Select(ToProto));
        response.ExpiredPossession.AddRange(expired.Select(ToProto));
        return Task.FromResult(response);
    }

    /// <summary>
    /// Grants every reached level of the mission pass not claimed yet: free rewards, plus premium rewards when the
    /// pass's premium item is owned (levels reached before buying it included).
    /// </summary>
    public override Task<ReceiveMissionPassRewardsResponse> ReceiveMissionPassRewards(ReceiveMissionPassRewardsRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        List<MissionRewardGrant> granted = MissionPassHelper.Claim(userDb, _masterDb, request.MissionPassId, nowMs);

        ReceiveMissionPassRewardsResponse response = new();
        response.ReceivedPossession.AddRange(granted.Select(g => new MissionPassReward { PossessionType = (int)g.PossessionType, PossessionId = g.PossessionId, Count = g.Count }));
        return Task.FromResult(response);
    }

    private static MissionReward ToProto(MissionRewardGrant grant)
        => new() { PossessionType = (int)grant.PossessionType, PossessionId = grant.PossessionId, Count = grant.Count };
}
