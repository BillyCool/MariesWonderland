using MariesWonderland.Proto.Battle;
using Grpc.Core;

namespace MariesWonderland.Services;

public class BattleService : MariesWonderland.Proto.Battle.BattleService.BattleServiceBase
{
    public override Task<StartWaveResponse> StartWave(StartWaveRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartWaveResponse());
    }

    public override Task<FinishWaveResponse> FinishWave(FinishWaveRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishWaveResponse());
    }
}
