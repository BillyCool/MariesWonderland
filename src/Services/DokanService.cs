using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Dokan;

namespace MariesWonderland.Services;

public class DokanService(UserDataStore store) : MariesWonderland.Proto.Dokan.DokanService.DokanServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Marks one or more dokan story chapters as seen by the player, recording the display timestamp.</summary>
    public override Task<RegisterDokanConfirmedResponse> RegisterDokanConfirmed(RegisterDokanConfirmedRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (int dokanId in request.DokanId)
        {
            EntityIUserDokan? existing = userDb.EntityIUserDokan
                .FirstOrDefault(d => d.DokanId == dokanId);

            if (existing == null)
            {
                userDb.EntityIUserDokan.Add(new EntityIUserDokan
                {
                    UserId = userId,
                    DokanId = dokanId,
                    DisplayDatetime = nowMs
                });
            }
            else
            {
                existing.DisplayDatetime = nowMs;
            }
        }

        return Task.FromResult(new RegisterDokanConfirmedResponse());
    }
}
