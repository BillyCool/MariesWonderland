using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Omikuji;

namespace MariesWonderland.Services;

public class OmikujiService(UserDataStore store, DarkMasterMemoryDatabase masterDb) : MariesWonderland.Proto.Omikuji.OmikujiService.OmikujiServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Draws a fortune (omikuji), records the draw timestamp, and returns the result asset ID.</summary>
    public override Task<OmikujiDrawResponse> OmikujiDraw(OmikujiDrawRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        int omikujiId = request.OmikujiId;

        EntityIUserOmikuji? existing = userDb.EntityIUserOmikuji.FirstOrDefault(o => o.OmikujiId == omikujiId);
        if (existing != null)
        {
            existing.LatestDrawDatetime = nowMs;
        }
        else
        {
            userDb.EntityIUserOmikuji.Add(new EntityIUserOmikuji { UserId = userId, OmikujiId = omikujiId, LatestDrawDatetime = nowMs });
        }

        EntityMOmikuji? masterOmikuji = _masterDb.EntityMOmikuji.FirstOrDefault(o => o.OmikujiId == omikujiId);
        OmikujiDrawResponse response = new() { OmikujiResultAssetId = masterOmikuji?.OmikujiAssetId ?? 0 };
        return Task.FromResult(response);
    }
}
