using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.ConsumableItem;

namespace MariesWonderland.Services;

public class ConsumableItemService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig, TimeProvider timeProvider)
    : MariesWonderland.Proto.ConsumableItem.ConsumableitemService.ConsumableitemServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Uses stamina or Arena BP recovery items. Fixed items add their value per item; percentage items a share of the
    /// level's stamina maximum or the BP cap. BP can go past its cap; stamina stops at the possession limit.
    /// </summary>
    public override Task<UseEffectItemResponse> UseEffectItem(UseEffectItemRequest request, ServerCallContext context)
    {
        if (request.Count <= 0)
        {
            return Task.FromResult(new UseEffectItemResponse());
        }

        EntityMConsumableItemEffect? effect = _masterDb.EntityMConsumableItemEffect
            .FirstOrDefault(itemEffect => itemEffect.ConsumableItemId == request.ConsumableItemId);
        if (effect == null || effect.EffectTargetType is not (EffectTargetType.STAMINA_RECOVERY or EffectTargetType.BATTLE_POINT_RECOVERY))
        {
            return Task.FromResult(new UseEffectItemResponse());
        }

        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase userDb))
        {
            return Task.FromResult(new UseEffectItemResponse());
        }

        EntityIUserConsumableItem? userItem = userDb.EntityIUserConsumableItem
            .FirstOrDefault(item => item.ConsumableItemId == request.ConsumableItemId);
        if (userItem == null || userItem.Count < request.Count)
        {
            return Task.FromResult(new UseEffectItemResponse());
        }

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        bool used = effect.EffectTargetType == EffectTargetType.BATTLE_POINT_RECOVERY
            ? RecoverBattlePoints(userDb, effect, request.Count, nowMs)
            : RecoverStamina(userDb, effect, request.Count, nowMs);
        if (!used)
        {
            return Task.FromResult(new UseEffectItemResponse());
        }

        userItem.Count -= request.Count;
        if (userItem.Count == 0)
        {
            userDb.EntityIUserConsumableItem.Remove(userItem);
        }

        return Task.FromResult(new UseEffectItemResponse());
    }

    private bool RecoverStamina(DarkUserMemoryDatabase userDb, EntityMConsumableItemEffect effect, int count, long nowMs)
    {
        EntityIUserStatus? userStatus = userDb.EntityIUserStatus.FirstOrDefault(status => status.UserId == userDb.UserId);
        if (userStatus == null)
        {
            return false;
        }

        int staminaCap = ResolveStaminaCap(userStatus.Level);
        int recoverAmount = RecoverAmount(effect, staminaCap);
        if (recoverAmount <= 0 || staminaCap <= 0)
        {
            return false;
        }

        StaminaHelper.Settle(userStatus, _masterDb, _gameConfig, nowMs);
        StaminaHelper.AddStamina(userStatus, recoverAmount * count, staminaCap, nowMs);
        return true;
    }

    private bool RecoverBattlePoints(DarkUserMemoryDatabase userDb, EntityMConsumableItemEffect effect, int count, long nowMs)
    {
        int recoverAmount = RecoverAmount(effect, _gameConfig.PvpMaxBattlePoint);
        if (recoverAmount <= 0)
        {
            return false;
        }

        EntityIUserPvpStatus status = userDb.EntityIUserPvpStatus.GetOrCreate(userDb.UserId);
        StaminaHelper.AddBattlePoints(status, recoverAmount * count, _gameConfig, nowMs);
        return true;
    }

    private static int RecoverAmount(EntityMConsumableItemEffect effect, int max) => effect.EffectValueType switch
    {
        EffectValueType.FIXED_VALUE => effect.EffectValue,
        EffectValueType.PERMIL_VALUE => max > 0 ? effect.EffectValue * max / 1000 : 0,
        _ => 0
    };

    /// <summary>Sells consumable items for gold, removing depleted entries from inventory.</summary>
    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        int totalGold = 0;

        foreach (SellPossession item in request.ConsumableItemPossession)
        {
            if (item.Count <= 0)
            {
                continue;
            }

            EntityMConsumableItem? masterRow = null;
            foreach (EntityMConsumableItem m in _masterDb.EntityMConsumableItem)
            {
                if (m.ConsumableItemId == item.ConsumableItemId)
                {
                    masterRow = m;
                    break;
                }
            }

            if (masterRow == null)
            {
                continue;
            }

            EntityIUserConsumableItem? userItem = null;
            foreach (EntityIUserConsumableItem u in userDb.EntityIUserConsumableItem)
            {
                if (u.ConsumableItemId == item.ConsumableItemId)
                {
                    userItem = u;
                    break;
                }
            }

            if (userItem == null || userItem.Count < item.Count)
            {
                continue;
            }

            userItem.Count -= item.Count;
            if (userItem.Count == 0)
            {
                userDb.EntityIUserConsumableItem.Remove(userItem);
            }

            totalGold += masterRow.SellPrice * item.Count;
        }

        if (totalGold > 0)
        {
            AddGold(userDb, totalGold, nowMs);
        }

        return Task.FromResult(new SellResponse());
    }

    /// <summary>Adds gold (consumable item ID 1) to the user's inventory, creating the entry if needed.</summary>
    private void AddGold(DarkUserMemoryDatabase userDb, int amount, long nowMs)
        => PossessionHelper.Apply(userDb, PossessionType.CONSUMABLE_ITEM, _gameConfig.ConsumableItemIdForGold, amount, _masterDb, nowMs);

    private int ResolveStaminaCap(int userLevel)
        => _masterDb.EntityMUserLevel.FirstOrDefault(level => level.UserLevel == userLevel)?.MaxStamina
            ?? _gameConfig.StaminaMaxCount;
}
