using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Companion;

namespace MariesWonderland.Services;

public class CompanionService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig, MissionTracker missions, TimeProvider timeProvider)
    : MariesWonderland.Proto.Companion.CompanionService.CompanionServiceBase
{
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;
    private const int CompanionMaxLevel = 50;

    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>Levels up a companion by deducting gold and materials per level, capping at the max companion level.</summary>
    public override Task<EnhanceResponse> Enhance(EnhanceRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        // Find the user's companion by UUID
        EntityIUserCompanion? companion = null;
        foreach (EntityIUserCompanion c in userDb.EntityIUserCompanion)
        {
            if (c.UserCompanionUuid == request.UserCompanionUuid)
            {
                companion = c;
                break;
            }
        }

        if (companion == null)
        {
            return Task.FromResult(new EnhanceResponse());
        }

        // Find companion master data
        EntityMCompanion? compDef = null;
        foreach (EntityMCompanion mc in _masterDb.EntityMCompanion)
        {
            if (mc.CompanionId == companion.CompanionId)
            {
                compDef = mc;
                break;
            }
        }

        if (compDef == null)
        {
            return Task.FromResult(new EnhanceResponse());
        }

        int targetLevel = companion.Level + request.AddLevelCount;
        if (targetLevel > CompanionMaxLevel)
        {
            targetLevel = CompanionMaxLevel;
        }

        // Find gold cost function for this category
        EntityMCompanionCategory? category = null;
        foreach (EntityMCompanionCategory cat in _masterDb.EntityMCompanionCategory)
        {
            if (cat.CompanionCategoryType == compDef.CompanionCategoryType)
            {
                category = cat;
                break;
            }
        }

        for (int lvl = companion.Level; lvl < targetLevel; lvl++)
        {
            // Deduct gold cost
            if (category != null)
            {
                int goldCost = NumericalFunctionHelper.Evaluate(_masterDb, category.EnhancementCostNumericalFunctionId, lvl);

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
                    gold.Count -= goldCost;
                }
            }

            // Deduct materials for this level
            foreach (EntityMCompanionEnhancementMaterial mat in _masterDb.EntityMCompanionEnhancementMaterial)
            {
                if (mat.CompanionCategoryType == compDef.CompanionCategoryType && mat.Level == lvl)
                {
                    EntityIUserMaterial? userMat = null;
                    foreach (EntityIUserMaterial m in userDb.EntityIUserMaterial)
                    {
                        if (m.MaterialId == mat.MaterialId)
                        {
                            userMat = m;
                            break;
                        }
                    }

                    if (userMat != null)
                    {
                        userMat.Count -= mat.Count;
                    }
                }
            }
        }

        companion.Level = targetLevel;

        _missions.Record(userDb, MissionClearConditionType.COMPANION_ENHANCE_BY_COUNT, 1, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());

        return Task.FromResult(new EnhanceResponse());
    }
}
