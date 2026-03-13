using MariesWonderland.Proto.Pvp;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class PvpService : MariesWonderland.Proto.Pvp.PvpService.PvpServiceBase
{
    public override Task<GetTopDataResponse> GetTopData(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetTopDataResponse());
    }

    public override Task<GetMatchingListResponse> GetMatchingList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetMatchingListResponse());
    }

    public override Task<UpdateMatchingListResponse> UpdateMatchingList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateMatchingListResponse());
    }

    public override Task<StartBattleResponse> StartBattle(StartBattleRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartBattleResponse());
    }

    public override Task<FinishBattleResponse> FinishBattle(FinishBattleRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishBattleResponse());
    }

    public override Task<GetRankingResponse> GetRanking(GetRankingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetRankingResponse());
    }

    public override Task<GetSeasonResultResponse> GetSeasonResult(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetSeasonResultResponse());
    }

    public override Task<GetAttackLogListResponse> GetAttackLogList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetAttackLogListResponse());
    }

    public override Task<GetDefenseLogListResponse> GetDefenseLogList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetDefenseLogListResponse());
    }
}
