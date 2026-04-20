using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Material;

namespace MariesWonderland.Services;

public class MaterialService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.Material.MaterialService.MaterialServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>Sells materials for gold, removing depleted material entries from inventory.</summary>
    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int totalGold = 0;

        foreach (SellPossession item in request.MaterialPossession)
        {
            // Look up the material's sell price from master data
            EntityMMaterial? mat = null;
            foreach (EntityMMaterial m in _masterDb.EntityMMaterial)
            {
                if (m.MaterialId == item.MaterialId)
                {
                    mat = m;
                    break;
                }
            }

            if (mat == null)
            {
                continue;
            }

            EntityIUserMaterial? userMat = null;
            foreach (EntityIUserMaterial um in userDb.EntityIUserMaterial)
            {
                if (um.MaterialId == item.MaterialId)
                {
                    userMat = um;
                    break;
                }
            }

            if (userMat == null || userMat.Count < item.Count)
            {
                continue;
            }

            userMat.Count -= item.Count;

            // If count reaches zero, remove the entry
            if (userMat.Count <= 0)
            {
                userDb.EntityIUserMaterial.Remove(userMat);
            }

            totalGold += mat.SellPrice * item.Count;
        }

        // Credit the total gold earned to the user's consumable inventory
        if (totalGold > 0)
        {
            EntityIUserConsumableItem? gold = null;
            foreach (EntityIUserConsumableItem ci in userDb.EntityIUserConsumableItem)
            {
                if (ci.ConsumableItemId == _gameConfig.ConsumableItemIdForGold)
                {
                    gold = ci;
                    break;
                }
            }

            if (gold != null)
            {
                gold.Count += totalGold;
            }
            else
            {
                userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
                {
                    UserId = userId,
                    ConsumableItemId = _gameConfig.ConsumableItemIdForGold,
                    Count = totalGold,
                    FirstAcquisitionDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
        }

        return Task.FromResult(new SellResponse());
    }
}
