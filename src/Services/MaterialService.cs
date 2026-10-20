using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
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
            if (item.Count <= 0)
            {
                continue;
            }

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
            GrantSaleBonus(userDb, mat, item.Count);
        }

        if (totalGold > 0)
        {
            PossessionHelper.Apply(userDb, PossessionType.CONSUMABLE_ITEM, _gameConfig.ConsumableItemIdForGold, totalGold, _masterDb);
        }

        return Task.FromResult(new SellResponse());
    }

    private void GrantSaleBonus(DarkUserMemoryDatabase userDb, EntityMMaterial material, int soldCount)
    {
        if (soldCount <= 0 || material.MaterialSaleObtainPossessionId <= 0)
        {
            return;
        }

        foreach (EntityMMaterialSaleObtainPossession bonus in _masterDb.EntityMMaterialSaleObtainPossession
                     .Where(entry => entry.MaterialSaleObtainPossessionId == material.MaterialSaleObtainPossessionId)
                     .OrderBy(entry => entry.SortOrder))
        {
            PossessionHelper.Apply(userDb, bonus.PossessionType, bonus.PossessionId, bonus.Count * soldCount, _masterDb);
        }
    }
}
