using MariesWonderland.Proto.BigHunt;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class BigHuntService : MariesWonderland.Proto.BigHunt.BighuntService.BighuntServiceBase
{
    public override Task<StartBigHuntQuestResponse> StartBigHuntQuest(StartBigHuntQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartBigHuntQuestResponse());
    }

    public override Task<UpdateBigHuntQuestSceneProgressResponse> UpdateBigHuntQuestSceneProgress(UpdateBigHuntQuestSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateBigHuntQuestSceneProgressResponse());
    }

    public override Task<FinishBigHuntQuestResponse> FinishBigHuntQuest(FinishBigHuntQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishBigHuntQuestResponse());
    }

    public override Task<RestartBigHuntQuestResponse> RestartBigHuntQuest(RestartBigHuntQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RestartBigHuntQuestResponse());
    }

    public override Task<SkipBigHuntQuestResponse> SkipBigHuntQuest(SkipBigHuntQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SkipBigHuntQuestResponse());
    }

    public override Task<SaveBigHuntBattleInfoResponse> SaveBigHuntBattleInfo(SaveBigHuntBattleInfoRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SaveBigHuntBattleInfoResponse());
    }

    public override Task<GetBigHuntTopDataResponse> GetBigHuntTopData(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetBigHuntTopDataResponse());
    }
}
