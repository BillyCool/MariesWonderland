using Grpc.Core;
using MariesWonderland.Proto.Gacha;
using MariesWonderland.Proto.GamePlay;

namespace MariesWonderland.Services;

public class GameplayService : MariesWonderland.Proto.GamePlay.GameplayService.GameplayServiceBase
{
    public override Task<CheckBeforeGamePlayResponse> CheckBeforeGamePlay(CheckBeforeGamePlayRequest request, ServerCallContext context)
    {
        CheckBeforeGamePlayResponse response = new()
        {
            IsExistUnreadPop = true
        };
        response.MenuGachaBadgeInfo.Add(new MenuGachaBadgeInfo()
        {
            DisplayStartDatetime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTimeOffset.MinValue),
            DisplayEndDatetime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTimeOffset.MaxValue),
        });

        return Task.FromResult(response);
    }
}
