using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.ConsumableItem;

namespace MariesWonderland.Services;

public class ConsumableItemService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.ConsumableItem.ConsumableitemService.ConsumableitemServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>Returns an empty response. Consumable item use effects not yet implemented.</summary>
    public override Task<UseEffectItemResponse> UseEffectItem(UseEffectItemRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UseEffectItemResponse());
    }

    /// <summary>Sells consumable items for gold, removing depleted entries from inventory.</summary>
    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int totalGold = 0;

        foreach (SellPossession item in request.ConsumableItemPossession)
        {
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
            AddGold(userDb, userId, totalGold);
        }

        return Task.FromResult(new SellResponse());
    }

    /// <summary>Adds gold (consumable item ID 1) to the user's inventory, creating the entry if needed.</summary>
    private void AddGold(DarkUserMemoryDatabase userDb, long userId, int amount)
    {
        foreach (EntityIUserConsumableItem ci in userDb.EntityIUserConsumableItem)
        {
            if (ci.ConsumableItemId == _gameConfig.ConsumableItemIdForGold)
            {
                ci.Count += amount;
                return;
            }
        }

        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = userId,
            ConsumableItemId = _gameConfig.ConsumableItemIdForGold,
            Count = amount,
            FirstAcquisitionDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }
}
