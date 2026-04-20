using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Shop;

namespace MariesWonderland.Services;

public class ShopService(UserDataStore store, DarkMasterMemoryDatabase masterDb) : MariesWonderland.Proto.Shop.ShopService.ShopServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Purchases shop items: deducts currency, grants item contents and effects, and updates purchase history.</summary>
    public override Task<BuyResponse> Buy(BuyRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach ((int shopItemId, int qty) in request.ShopItems)
        {
            // Validate item exists in master data
            EntityMShopItem? item = _masterDb.EntityMShopItem.FirstOrDefault(i => i.ShopItemId == shopItemId);
            if (item == null) { continue; }

            // Deduct the total price; skip this item if funds are insufficient
            int totalPrice = item.Price * qty;
            if (!DeductPrice(userDb, item.PriceType, item.PriceId, totalPrice)) { continue; }

            // Grant all possession contents for this shop item
            List<EntityMShopItemContentPossession> contents = [.. _masterDb.EntityMShopItemContentPossession
                .Where(c => c.ShopItemId == shopItemId)];

            foreach (EntityMShopItemContentPossession content in contents)
            {
                GrantShopPossession(userDb, userId, content.PossessionType, content.PossessionId, content.Count * qty);
            }

            // Apply side effects (e.g., stamina recovery)
            ApplyContentEffects(userDb, shopItemId, qty, userId, nowMs);

            // Track purchase count
            EntityIUserShopItem? shopItem = userDb.EntityIUserShopItem.FirstOrDefault(s => s.ShopItemId == shopItemId);
            if (shopItem == null)
            {
                shopItem = new EntityIUserShopItem { UserId = userId, ShopItemId = shopItemId };
                userDb.EntityIUserShopItem.Add(shopItem);
            }
            shopItem.BoughtCount += qty;
            shopItem.LatestBoughtCountChangedDatetime = nowMs;
        }

        return Task.FromResult(new BuyResponse());
    }

    /// <summary>
    /// Returns the sorted item shop pool (item IDs) from the ITEM_SHOP group's cell layout.
    /// </summary>
    private List<int> GetItemShopPool()
    {
        EntityMShop? itemShop = _masterDb.EntityMShop.FirstOrDefault(s => s.ShopGroupType == ShopGroupType.ITEM_SHOP);
        if (itemShop == null) { return []; }

        Dictionary<int, int> cellIdToItemId = _masterDb.EntityMShopItemCell
            .ToDictionary(c => c.ShopItemCellId, c => c.ShopItemId);

        return [.. _masterDb.EntityMShopItemCellGroup
            .Where(cg => cg.ShopItemCellGroupId == itemShop.ShopItemCellGroupId)
            .OrderBy(cg => cg.SortOrder)
            .Select(cg => cellIdToItemId.GetValueOrDefault(cg.ShopItemCellId))
            .Where(id => id != 0)];
    }

    /// <summary>Returns an empty response. CESA age-rating spending limits not yet implemented.</summary>
    public override Task<GetCesaLimitResponse> GetCesaLimit(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetCesaLimitResponse());
    }

    /// <summary>Refreshes the replaceable item shop lineup. Initializes slots on first call; resets purchase counts when gems are spent to refresh.</summary>
    public override Task<RefreshResponse> RefreshUserData(RefreshRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        List<int> itemShopPool = GetItemShopPool();

        if (!userDb.EntityIUserShopReplaceableLineup.Any(l => l.UserId == userId) && itemShopPool.Count > 0)
        {
            for (int i = 0; i < itemShopPool.Count; i++)
            {
                int slot = i + 1;
                userDb.EntityIUserShopReplaceableLineup.Add(new EntityIUserShopReplaceableLineup
                {
                    UserId = userId,
                    SlotNumber = slot,
                    ShopItemId = itemShopPool[i],
                });
            }
        }

        if (request.IsGemUsed)
        {
            EntityIUserShopReplaceable? replaceable = userDb.EntityIUserShopReplaceable.FirstOrDefault(r => r.UserId == userId);
            if (replaceable == null)
            {
                replaceable = new EntityIUserShopReplaceable { UserId = userId };
                userDb.EntityIUserShopReplaceable.Add(replaceable);
            }
            replaceable.LineupUpdateCount++;
            replaceable.LatestLineupUpdateDatetime = nowMs;

            foreach (int itemId in itemShopPool)
            {
                EntityIUserShopItem? si = userDb.EntityIUserShopItem.FirstOrDefault(s => s.ShopItemId == itemId);
                if (si != null)
                {
                    si.BoughtCount = 0;
                }
            }
        }

        return Task.FromResult(new RefreshResponse());
    }

    /// <summary>Creates a purchase transaction for real-money IAP: deducts currency, grants contents and effects, and returns a transaction ID.</summary>
    public override Task<CreatePurchaseTransactionResponse> CreatePurchaseTransaction(CreatePurchaseTransactionRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMShopItem? item = _masterDb.EntityMShopItem.FirstOrDefault(i => i.ShopItemId == request.ShopItemId);
        if (item != null)
        {
            DeductPrice(userDb, item.PriceType, item.PriceId, item.Price);

            List<EntityMShopItemContentPossession> contents = [.. _masterDb.EntityMShopItemContentPossession
                .Where(c => c.ShopItemId == request.ShopItemId)];

            foreach (EntityMShopItemContentPossession content in contents)
            {
                GrantShopPossession(userDb, userId, content.PossessionType, content.PossessionId, content.Count);
            }

            ApplyContentEffects(userDb, request.ShopItemId, 1, userId, nowMs);

            EntityIUserShopItem? shopItem = userDb.EntityIUserShopItem.FirstOrDefault(s => s.ShopItemId == request.ShopItemId);
            if (shopItem == null)
            {
                shopItem = new EntityIUserShopItem { UserId = userId, ShopItemId = request.ShopItemId };
                userDb.EntityIUserShopItem.Add(shopItem);
            }
            shopItem.BoughtCount++;

            if (item.ShopItemLimitedStockId > 0)
            {
                EntityMShopItemLimitedStock? stock = _masterDb.EntityMShopItemLimitedStock
                    .FirstOrDefault(s => s.ShopItemLimitedStockId == item.ShopItemLimitedStockId);
                if (stock != null && shopItem.BoughtCount >= stock.MaxCount)
                {
                    shopItem.BoughtCount = 0;
                }
            }

            shopItem.LatestBoughtCountChangedDatetime = nowMs;
        }

        string txId = $"tx_{userId}_{request.ShopItemId}_{nowMs}";

        return Task.FromResult(new CreatePurchaseTransactionResponse
        {
            PurchaseTransactionId = txId,
        });
    }

    /// <summary>Returns an empty response. Google Play Store IAP verification not yet implemented.</summary>
    public override Task<PurchaseGooglePlayStoreProductResponse> PurchaseGooglePlayStoreProduct(PurchaseGooglePlayStoreProductRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PurchaseGooglePlayStoreProductResponse());
    }

    /// <summary>
    /// Deducts the given price from the user's balance. Returns false if insufficient funds.
    /// GEM deducts free gems first, then paid gems. PAID_GEM deducts paid gems only.
    /// CONSUMABLE_ITEM deducts from the consumable with PriceId.
    /// </summary>
    private static bool DeductPrice(DarkUserMemoryDatabase userDb, PriceType priceType, int priceId, int amount)
    {
        switch (priceType)
        {
            case PriceType.GEM:
                {
                    EntityIUserGem? gem = userDb.EntityIUserGem.FirstOrDefault();
                    if (gem == null || gem.FreeGem + gem.PaidGem < amount) { return false; }
                    int fromFree = Math.Min(gem.FreeGem, amount);
                    gem.FreeGem -= fromFree;
                    gem.PaidGem -= amount - fromFree;
                    return true;
                }
            case PriceType.PAID_GEM:
                {
                    EntityIUserGem? gem = userDb.EntityIUserGem.FirstOrDefault();
                    if (gem == null || gem.PaidGem < amount) { return false; }
                    gem.PaidGem -= amount;
                    return true;
                }
            case PriceType.CONSUMABLE_ITEM:
                {
                    EntityIUserConsumableItem? item = userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == priceId);
                    if (item == null || item.Count < amount) { return false; }
                    item.Count -= amount;
                    return true;
                }
            default:
                return true;
        }
    }

    /// <summary>
    /// Grants a shop possession to the user. For costumes already owned, grants duplication
    /// exchange items instead. All other types delegate to PossessionHelper.Apply.
    /// </summary>
    private void GrantShopPossession(DarkUserMemoryDatabase userDb, long userId, PossessionType possessionType, int possessionId, int count)
    {
        if (possessionType is PossessionType.COSTUME or PossessionType.COSTUME_ENHANCED
            && userDb.EntityIUserCostume.Any(c => c.CostumeId == possessionId))
        {
            foreach (EntityMCostumeDuplicationExchangePossessionGroup exchange in _masterDb.EntityMCostumeDuplicationExchangePossessionGroup
                .Where(e => e.CostumeId == possessionId))
            {
                PossessionHelper.Apply(userDb, userId, exchange.PossessionType, exchange.PossessionId, exchange.Count * count, _masterDb);
            }
            return;
        }

        PossessionHelper.Apply(userDb, userId, possessionType, possessionId, count, _masterDb);
    }

    /// <summary>Applies side-effect content (e.g., stamina recovery) from a shop item purchase.</summary>
    private void ApplyContentEffects(DarkUserMemoryDatabase userDb, int shopItemId, int qty, long userId, long nowMs)
    {
        List<EntityMShopItemContentEffect> effects = [.. _masterDb.EntityMShopItemContentEffect
            .Where(e => e.ShopItemId == shopItemId)];

        EntityIUserStatus? userStatus = userDb.EntityIUserStatus.FirstOrDefault(s => s.UserId == userId);
        if (userStatus == null) { return; }

        EntityMUserLevel? levelData = _masterDb.EntityMUserLevel.FirstOrDefault(l => l.UserLevel == userStatus.Level);
        int maxStaminaMillis = (levelData?.MaxStamina ?? 0) * 1000;

        foreach (EntityMShopItemContentEffect effect in effects)
        {
            if (effect.EffectTargetType != EffectTargetType.STAMINA_RECOVERY) { continue; }

            int effectMillis = effect.EffectValueType switch
            {
                EffectValueType.FIXED_VALUE => effect.EffectValue,
                EffectValueType.PERMIL_VALUE => maxStaminaMillis > 0 ? effect.EffectValue * maxStaminaMillis / 1000 : 0,
                _ => 0
            };

            userStatus.StaminaMilliValue = Math.Min(userStatus.StaminaMilliValue + effectMillis * qty, maxStaminaMillis);
            userStatus.StaminaUpdateDatetime = nowMs;
        }
    }
}
