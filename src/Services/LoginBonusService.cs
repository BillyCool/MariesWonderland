using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.LoginBonus;

namespace MariesWonderland.Services;

public class LoginBonusService(UserDataStore store, DarkMasterMemoryDatabase masterDb, TimeProvider timeProvider)
    : MariesWonderland.Proto.LoginBonus.LoginbonusService.LoginbonusServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Claims the daily login stamp reward and delivers it to the player's gift inbox.</summary>
    public override Task<ReceiveStampResponse> ReceiveStamp(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserLoginBonus loginBonus = userDb.EntityIUserLoginBonus.FirstOrDefault(lb => lb.UserId == userId)
            ?? userDb.EntityIUserLoginBonus.GetOrCreate(
                entry => entry.UserId == userId && entry.LoginBonusId == 1,
                () => new EntityIUserLoginBonus
                {
                    UserId = userId,
                    LoginBonusId = 1,
                    CurrentPageNumber = 1,
                    CurrentStampNumber = 0
                });

        (int nextPage, int nextStamp, EntityMLoginBonusStamp stamp) = ResolveNextStamp(loginBonus);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        // TODO: Review the gift lifetime; 30 days is an assumption, not from master data or the real game.
        long expiryMs = nowMs + 30L * 24 * 60 * 60 * 1000;

        userDb.EntitySUserGift.Add(new EntitySUserGift
        {
            UserId = userId,
            UserGiftUuid = $"login-bonus-{userId}-{nextPage}-{nextStamp}",
            PossessionType = (int)stamp.RewardPossessionType,
            PossessionId = stamp.RewardPossessionId,
            Count = stamp.RewardCount,
            GrantDatetime = nowMs,
            ExpirationDatetime = expiryMs,
            ReceivedDatetime = 0
        });

        loginBonus.CurrentPageNumber = nextPage;
        loginBonus.CurrentStampNumber = nextStamp;
        loginBonus.LatestRewardReceiveDatetime = nowMs;

        return Task.FromResult(new ReceiveStampResponse());
    }

    private (int NextPage, int NextStamp, EntityMLoginBonusStamp Stamp) ResolveNextStamp(EntityIUserLoginBonus loginBonus)
    {
        int nextPage = loginBonus.CurrentPageNumber;
        int nextStamp = loginBonus.CurrentStampNumber + 1;

        EntityMLoginBonusStamp? stamp = FindStamp(loginBonus.LoginBonusId, nextPage, nextStamp);
        if (stamp is not null)
        {
            return (nextPage, nextStamp, stamp);
        }

        nextPage = loginBonus.CurrentPageNumber + 1;
        nextStamp = 1;

        EntityMLoginBonus? masterBonus = _masterDb.EntityMLoginBonus
            .FirstOrDefault(entry => entry.LoginBonusId == loginBonus.LoginBonusId);

        if (masterBonus is not null
            && masterBonus.TotalPageCount > 0
            && nextPage > masterBonus.TotalPageCount)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"login bonus {loginBonus.LoginBonusId} exhausted at page {loginBonus.CurrentPageNumber} stamp {loginBonus.CurrentStampNumber}"));
        }

        stamp = FindStamp(loginBonus.LoginBonusId, nextPage, nextStamp);
        if (stamp is null)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"no reward found for login bonus {loginBonus.LoginBonusId} page {nextPage} stamp {nextStamp}"));
        }

        return (nextPage, nextStamp, stamp);
    }

    private EntityMLoginBonusStamp? FindStamp(int loginBonusId, int pageNumber, int stampNumber) =>
        _masterDb.EntityMLoginBonusStamp.FirstOrDefault(s =>
            s.LoginBonusId == loginBonusId
            && s.LowerPageNumber == pageNumber
            && s.StampNumber == stampNumber);
}
