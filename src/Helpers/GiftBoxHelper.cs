using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>Sends rewards to the player's gift box, to be claimed from there.</summary>
public static class GiftBoxHelper
{
    /// <summary>Adds an unclaimed gift that expires after the usual period. <paramref name="descriptionGiftTextId"/> is the gift text (<c>m_gift_text</c>) shown under it; <paramref name="giftUuid"/> a fixed id for gifts that must not repeat (a random one by default).</summary>
    public static void Add(DarkUserMemoryDatabase userDb, PossessionType possessionType, int possessionId, int count, long nowMs, int descriptionGiftTextId = 0, string? giftUuid = null)
    {
        userDb.EntitySUserGift.Add(new EntitySUserGift
        {
            UserId = userDb.UserId,
            UserGiftUuid = giftUuid ?? Guid.NewGuid().ToString(),
            PossessionType = (int)possessionType,
            PossessionId = possessionId,
            Count = count,
            GrantDatetime = nowMs,
            DescriptionGiftTextId = descriptionGiftTextId,
            ExpirationDatetime = nowMs + GameConfig.GiftExpirationDays * 24L * 60 * 60 * 1000
        });
    }
}
