using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Gift;

namespace MariesWonderland.Services;

public class GiftService(UserDataStore store) : MariesWonderland.Proto.Gift.GiftService.GiftServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Claims inbox gifts by UUID, marking them as received with the current timestamp.</summary>
    public override Task<ReceiveGiftResponse> ReceiveGift(ReceiveGiftRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        List<string> received = [];

        foreach (EntitySUserGift gift in userDb.EntitySUserGift)
        {
            if (gift.UserId == userId
                && gift.ReceivedDatetime == 0
                && request.UserGiftUuid.Contains(gift.UserGiftUuid))
            {
                gift.ReceivedDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                received.Add(gift.UserGiftUuid);
            }
        }

        ReceiveGiftResponse response = new();
        response.ReceivedGiftUuid.AddRange(received);
        return Task.FromResult(response);
    }

    /// <summary>Returns a paginated list of unclaimed gifts, sorted by expiration date.</summary>
    public override Task<GetGiftListResponse> GetGiftList(GetGiftListRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        List<EntitySUserGift> unreceived = [.. userDb.EntitySUserGift
            .Where(g => g.ReceivedDatetime == 0)];

        if (request.IsAscendingSort)
        {
            unreceived.Sort((a, b) => a.ExpirationDatetime.CompareTo(b.ExpirationDatetime));
        }
        else
        {
            unreceived.Sort((a, b) => b.ExpirationDatetime.CompareTo(a.ExpirationDatetime));
        }

        IEnumerable<EntitySUserGift> page = unreceived;
        if (request.GetCount > 0)
        {
            page = page.Take(request.GetCount);
        }

        GetGiftListResponse response = new()
        {
            TotalPageCount = PageCount(unreceived.Count, request.GetCount),
            NextCursor = 0,
            PreviousCursor = 0
        };

        foreach (EntitySUserGift gift in page)
        {
            NotReceivedGift item = new()
            {
                GiftCommon = ToProtoGiftCommon(gift),
                UserGiftUuid = gift.UserGiftUuid
            };
            if (gift.ExpirationDatetime > 0)
            {
                item.ExpirationDatetime = Timestamp.FromDateTimeOffset(
                    DateTimeOffset.FromUnixTimeMilliseconds(gift.ExpirationDatetime));
            }
            response.Gift.Add(item);
        }

        return Task.FromResult(response);
    }

    /// <summary>Returns the history of previously claimed gifts.</summary>
    public override Task<GetGiftReceiveHistoryListResponse> GetGiftReceiveHistoryList(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        GetGiftReceiveHistoryListResponse response = new();

        foreach (EntitySUserGift gift in userDb.EntitySUserGift.Where(g => g.ReceivedDatetime != 0))
        {
            response.Gift.Add(new ReceivedGift
            {
                GiftCommon = ToProtoGiftCommon(gift),
                ReceivedDatetime = Timestamp.FromDateTimeOffset(
                    DateTimeOffset.FromUnixTimeMilliseconds(gift.ReceivedDatetime))
            });
        }

        return Task.FromResult(response);
    }

    /// <summary>Converts a server-side gift entity to the proto GiftCommon message.</summary>
    private static GiftCommon ToProtoGiftCommon(EntitySUserGift gift)
    {
        GiftCommon common = new()
        {
            PossessionType = gift.PossessionType,
            PossessionId = gift.PossessionId,
            Count = gift.Count,
            DescriptionGiftTextId = gift.DescriptionGiftTextId,
            EquipmentData = gift.EquipmentData.Length > 0
                ? ByteString.CopyFrom(gift.EquipmentData)
                : ByteString.Empty
        };
        if (gift.GrantDatetime > 0)
        {
            common.GrantDatetime = Timestamp.FromDateTimeOffset(
                DateTimeOffset.FromUnixTimeMilliseconds(gift.GrantDatetime));
        }
        return common;
    }

    /// <summary>Calculates the total number of pages given item count and page size.</summary>
    private static int PageCount(int total, int pageSize)
    {
        if (total == 0) return 0;
        if (pageSize <= 0) return 1;
        int pages = total / pageSize;
        if (total % pageSize != 0) pages++;
        return pages;
    }
}

