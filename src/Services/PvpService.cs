using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Proto.Pvp;

namespace MariesWonderland.Services;

public class PvpService : MariesWonderland.Proto.Pvp.PvpService.PvpServiceBase
{
    /// <summary>Returns an empty response. PvP top screen not yet implemented.</summary>
    public override Task<GetTopDataResponse> GetTopData(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetTopDataResponse());
    }

    /// <summary>Returns an empty response. PvP matchmaking not yet implemented.</summary>
    public override Task<GetMatchingListResponse> GetMatchingList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetMatchingListResponse());
    }

    /// <summary>Returns an empty response. PvP matchmaking not yet implemented.</summary>
    public override Task<UpdateMatchingListResponse> UpdateMatchingList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateMatchingListResponse());
    }

    /// <summary>Returns an empty response. PvP battles not yet implemented.</summary>
    public override Task<StartBattleResponse> StartBattle(StartBattleRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartBattleResponse());
    }

    /// <summary>Returns an empty response. PvP battles not yet implemented.</summary>
    public override Task<FinishBattleResponse> FinishBattle(FinishBattleRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishBattleResponse());
    }

    /// <summary>Returns an empty response. PvP rankings not yet implemented.</summary>
    public override Task<GetRankingResponse> GetRanking(GetRankingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetRankingResponse());
    }

    /// <summary>Returns an empty response. PvP season results not yet implemented.</summary>
    public override Task<GetSeasonResultResponse> GetSeasonResult(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetSeasonResultResponse());
    }

    /// <summary>Returns an empty response. PvP attack logs not yet implemented.</summary>
    public override Task<GetAttackLogListResponse> GetAttackLogList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetAttackLogListResponse());
    }

    /// <summary>Returns an empty response. PvP defense logs not yet implemented.</summary>
    public override Task<GetDefenseLogListResponse> GetDefenseLogList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetDefenseLogListResponse());
    }
}
