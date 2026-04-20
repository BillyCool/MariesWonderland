using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.LoginBonus;

namespace MariesWonderland.Services;

public class LoginBonusService(UserDataStore store, DarkMasterMemoryDatabase masterDb)
    : MariesWonderland.Proto.LoginBonus.LoginbonusService.LoginbonusServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Claims the daily login stamp reward and delivers it to the player's gift inbox.</summary>
    public override Task<ReceiveStampResponse> ReceiveStamp(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        // Get or create login bonus tracker, defaulting to page 1 / stamp 0
        EntityIUserLoginBonus loginBonus = userDb.EntityIUserLoginBonus.FirstOrDefault(lb => lb.UserId == userId)
            ?? AddEntity(userDb.EntityIUserLoginBonus, new EntityIUserLoginBonus
            {
                UserId = userId,
                LoginBonusId = 1,
                CurrentPageNumber = 1,
                CurrentStampNumber = 0
            });

        int nextStamp = loginBonus.CurrentStampNumber + 1;

        // Look up the reward for the next stamp from master data
        EntityMLoginBonusStamp? stamp = _masterDb.EntityMLoginBonusStamp.FirstOrDefault(s =>
            s.LoginBonusId == loginBonus.LoginBonusId
            && s.LowerPageNumber == loginBonus.CurrentPageNumber
            && s.StampNumber == nextStamp);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long expiryMs = nowMs + 30L * 24 * 60 * 60 * 1000;

        // Deliver the stamp reward as a gift with 30-day expiry
        if (stamp is not null)
        {
            userDb.EntitySUserGift.Add(new EntitySUserGift
            {
                UserId = userId,
                UserGiftUuid = $"login-bonus-{userId}-{nextStamp}",
                PossessionType = (int)stamp.RewardPossessionType,
                PossessionId = stamp.RewardPossessionId,
                Count = stamp.RewardCount,
                GrantDatetime = nowMs,
                ExpirationDatetime = expiryMs,
                ReceivedDatetime = 0
            });
        }

        loginBonus.CurrentStampNumber = nextStamp;
        loginBonus.LatestRewardReceiveDatetime = nowMs;

        return Task.FromResult(new ReceiveStampResponse());
    }

    /// <summary>Adds an entity to a list and returns it, enabling inline initialization with null-coalescing.</summary>
    private static T AddEntity<T>(List<T> list, T entity)
    {
        list.Add(entity);
        return entity;
    }
}

