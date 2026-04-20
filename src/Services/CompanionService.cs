using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Companion;

namespace MariesWonderland.Services;

public class CompanionService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.Companion.CompanionService.CompanionServiceBase
{
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
                int goldCost = EvaluateNumericalFunction(category.EnhancementCostNumericalFunctionId, lvl);

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

        return Task.FromResult(new EnhanceResponse());
    }

    /// <summary>Evaluates a master data numerical function (linear, monomial, polynomial) for a given input value.</summary>
    private int EvaluateNumericalFunction(int functionId, int value)
    {
        EntityMNumericalFunction? func = null;
        foreach (EntityMNumericalFunction f in _masterDb.EntityMNumericalFunction)
        {
            if (f.NumericalFunctionId == functionId)
            {
                func = f;
                break;
            }
        }

        if (func == null)
        {
            return 0;
        }

        List<(int Index, int Value)> paramEntries = [];
        foreach (EntityMNumericalFunctionParameterGroup pg in _masterDb.EntityMNumericalFunctionParameterGroup)
        {
            if (pg.NumericalFunctionParameterGroupId == func.NumericalFunctionParameterGroupId)
            {
                paramEntries.Add((pg.ParameterIndex, pg.ParameterValue));
            }
        }
        paramEntries.Sort((a, b) => a.Index.CompareTo(b.Index));

        int[] p = new int[paramEntries.Count];
        for (int i = 0; i < paramEntries.Count; i++)
        {
            p[i] = paramEntries[i].Value;
        }

        return func.NumericalFunctionType switch
        {
            NumericalFunctionType.LINEAR when p.Length >= 2 => p[1] + p[0] * value,
            NumericalFunctionType.MONOMIAL when p.Length >= 2 => EvaluateMonomial(p, value),
            NumericalFunctionType.LINEAR_PERMIL when p.Length >= 2 => p[0] * value / 1000 + p[1],
            NumericalFunctionType.POLYNOMIAL_THIRD when p.Length >= 4 => p[3] + (p[2] + (p[1] + p[0] * value) * value) * value,
            NumericalFunctionType.POLYNOMIAL_THIRD_PERMIL when p.Length >= 4 =>
                p[0] * value * value * value / 1000 +
                p[1] * value * value / 1000 +
                p[2] * value / 1000 +
                p[3],
            _ => 0
        };
    }

    /// <summary>Evaluates a monomial function: p[0] * (value - 1) ^ p[1].</summary>
    private static int EvaluateMonomial(int[] p, int value)
    {
        int v = value - 1;
        int result = v;
        int counter = p[1];
        if (counter > 1)
        {
            counter--;
            while (counter > 0)
            {
                counter--;
                result *= v;
            }
        }
        return result * p[0];
    }
}
