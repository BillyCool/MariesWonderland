using MariesWonderland.Proto.Quest;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class QuestService : MariesWonderland.Proto.Quest.QuestService.QuestServiceBase
{
    public override Task<UpdateMainFlowSceneProgressResponse> UpdateMainFlowSceneProgress(UpdateMainFlowSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateMainFlowSceneProgressResponse());
    }

    public override Task<UpdateReplayFlowSceneProgressResponse> UpdateReplayFlowSceneProgress(UpdateReplayFlowSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateReplayFlowSceneProgressResponse());
    }

    public override Task<UpdateMainQuestSceneProgressResponse> UpdateMainQuestSceneProgress(UpdateMainQuestSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateMainQuestSceneProgressResponse());
    }

    public override Task<UpdateExtraQuestSceneProgressResponse> UpdateExtraQuestSceneProgress(UpdateExtraQuestSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateExtraQuestSceneProgressResponse());
    }

    public override Task<UpdateEventQuestSceneProgressResponse> UpdateEventQuestSceneProgress(UpdateEventQuestSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateEventQuestSceneProgressResponse());
    }

    public override Task<StartMainQuestResponse> StartMainQuest(StartMainQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartMainQuestResponse());
    }

    public override Task<RestartMainQuestResponse> RestartMainQuest(RestartMainQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RestartMainQuestResponse());
    }

    public override Task<FinishMainQuestResponse> FinishMainQuest(FinishMainQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishMainQuestResponse());
    }

    public override Task<StartExtraQuestResponse> StartExtraQuest(StartExtraQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartExtraQuestResponse());
    }

    public override Task<RestartExtraQuestResponse> RestartExtraQuest(RestartExtraQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RestartExtraQuestResponse());
    }

    public override Task<FinishExtraQuestResponse> FinishExtraQuest(FinishExtraQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishExtraQuestResponse());
    }

    public override Task<StartEventQuestResponse> StartEventQuest(StartEventQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartEventQuestResponse());
    }

    public override Task<RestartEventQuestResponse> RestartEventQuest(RestartEventQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RestartEventQuestResponse());
    }

    public override Task<FinishEventQuestResponse> FinishEventQuest(FinishEventQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishEventQuestResponse());
    }

    public override Task<FinishAutoOrbitResponse> FinishAutoOrbit(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new FinishAutoOrbitResponse());
    }

    public override Task<SetRouteResponse> SetRoute(SetRouteRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetRouteResponse());
    }

    public override Task<SetQuestSceneChoiceResponse> SetQuestSceneChoice(SetQuestSceneChoiceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetQuestSceneChoiceResponse());
    }

    public override Task<ReceiveTowerAccumulationRewardResponse> ReceiveTowerAccumulationReward(ReceiveTowerAccumulationRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveTowerAccumulationRewardResponse());
    }

    public override Task<SkipQuestResponse> SkipQuest(SkipQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SkipQuestResponse());
    }

    public override Task<SkipQuestBulkResponse> SkipQuestBulk(SkipQuestBulkRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SkipQuestBulkResponse());
    }

    public override Task<SetAutoSaleSettingResponse> SetAutoSaleSetting(SetAutoSaleSettingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetAutoSaleSettingResponse());
    }

    public override Task<StartGuerrillaFreeOpenResponse> StartGuerrillaFreeOpen(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new StartGuerrillaFreeOpenResponse());
    }

    public override Task<ResetLimitContentQuestProgressResponse> ResetLimitContentQuestProgress(ResetLimitContentQuestProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ResetLimitContentQuestProgressResponse());
    }

    public override Task<ReceiveDailyQuestGroupCompleteRewardResponse> ReceiveDailyQuestGroupCompleteReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveDailyQuestGroupCompleteRewardResponse());
    }
}
