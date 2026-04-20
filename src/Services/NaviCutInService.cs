using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.NaviCutIn;

namespace MariesWonderland.Services;

public class NaviCutInService(UserDataStore store)
    : MariesWonderland.Proto.NaviCutIn.NavicutinService.NavicutinServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Records that a navi cut-in animation has been played, so the client won't show it again.</summary>
    public override Task<RegisterPlayedResponse> RegisterPlayed(RegisterPlayedRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserNaviCutIn? record = userDb.EntityIUserNaviCutIn
            .FirstOrDefault(n => n.NaviCutInId == request.NaviCutId);

        if (record is null)
        {
            userDb.EntityIUserNaviCutIn.Add(new EntityIUserNaviCutIn
            {
                UserId = userId,
                NaviCutInId = request.NaviCutId,
                PlayDatetime = nowMs
            });
        }
        else
        {
            record.PlayDatetime = nowMs;
        }

        return Task.FromResult(new RegisterPlayedResponse());
    }
}
