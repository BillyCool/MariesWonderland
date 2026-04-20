using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Costume;

namespace MariesWonderland.Services;

public class CostumeService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.Costume.CostumeService.CostumeServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>
    /// Enhances a costume using materials: deducts materials and gold, adds EXP, recalculates level.
    /// </summary>
    /// <summary>
    /// Enhances a costume using enhancement materials to gain EXP. Materials matching the costume's weapon type grant a 1.5x EXP bonus.
    /// </summary>
    public override Task<EnhanceResponse> Enhance(EnhanceRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserCostume? costume = null;
        foreach (EntityIUserCostume c in userDb.EntityIUserCostume)
        {
            if (c.UserCostumeUuid == request.UserCostumeUuid)
            {
                costume = c;
                break;
            }
        }

        if (costume == null)
        {
            return Task.FromResult(new EnhanceResponse());
        }

        EntityMCostume? costumeMaster = null;
        foreach (EntityMCostume cm in _masterDb.EntityMCostume)
        {
            if (cm.CostumeId == costume.CostumeId)
            {
                costumeMaster = cm;
                break;
            }
        }

        if (costumeMaster == null)
        {
            return Task.FromResult(new EnhanceResponse());
        }

        // Filter master data to only costume-enhancement materials
        Dictionary<int, EntityMMaterial> materialCatalog = [];
        foreach (EntityMMaterial mat in _masterDb.EntityMMaterial)
        {
            if (mat.MaterialType == MaterialType.COSTUME_ENHANCEMENT)
            {
                materialCatalog[mat.MaterialId] = mat;
            }
        }

        // Consume materials and calculate total EXP gained
        int totalExp = 0;
        int totalMaterialCount = 0;

        foreach (KeyValuePair<int, int> entry in request.Materials)
        {
            int materialId = entry.Key;
            int count = entry.Value;

            if (!materialCatalog.TryGetValue(materialId, out EntityMMaterial? mat))
            {
                continue;
            }

            EntityIUserMaterial? userMat = null;
            foreach (EntityIUserMaterial m in userDb.EntityIUserMaterial)
            {
                if (m.MaterialId == materialId)
                {
                    userMat = m;
                    break;
                }
            }

            if (userMat == null || userMat.Count < count)
            {
                continue;
            }

            userMat.Count -= count;
            totalMaterialCount += count;

            // Apply 1.5x EXP bonus when material weapon type matches the costume's proficient weapon type
            int expPerUnit = mat.EffectValue;
            if (mat.WeaponType != WeaponType.UNKNOWN && mat.WeaponType == costumeMaster.SkillfulWeaponType)
            {
                expPerUnit = expPerUnit * _gameConfig.MaterialSameWeaponExpCoefficientPermil / 1000;
            }

            totalExp += expPerUnit * count;
        }

        // Look up rarity-based cost and EXP threshold parameters
        EntityMCostumeRarity? rarityRow = null;
        foreach (EntityMCostumeRarity r in _masterDb.EntityMCostumeRarity)
        {
            if (r.RarityType == costumeMaster.RarityType)
            {
                rarityRow = r;
                break;
            }
        }

        // Deduct gold cost scaled by number of materials used
        if (totalMaterialCount > 0 && rarityRow != null)
        {
            int goldCost = EvaluateNumericalFunction(rarityRow.EnhancementCostByMaterialNumericalFunctionId, totalMaterialCount);

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

        // Apply EXP and recalculate level from rarity-specific thresholds
        costume.Exp += totalExp;

        if (rarityRow != null)
        {
            (costume.Level, costume.Exp) = CalculateCostumeLevelAndCap(costume.Exp, rarityRow.RequiredExpForLevelUpNumericalParameterMapId);
        }

        return Task.FromResult(new EnhanceResponse { IsGreatSuccess = false });
    }

    /// <summary>
    /// Calculates the costume level from accumulated EXP and caps EXP at the max threshold.
    /// </summary>
    private (int Level, int CappedExp) CalculateCostumeLevelAndCap(int exp, int paramMapId)
    {
        int maxKey = 0;
        foreach (EntityMNumericalParameterMap row in _masterDb.EntityMNumericalParameterMap)
        {
            if (row.NumericalParameterMapId == paramMapId && row.ParameterKey > maxKey)
            {
                maxKey = row.ParameterKey;
            }
        }

        int[] thresholds = new int[maxKey + 1];
        foreach (EntityMNumericalParameterMap row in _masterDb.EntityMNumericalParameterMap)
        {
            if (row.NumericalParameterMapId == paramMapId && row.ParameterKey < thresholds.Length)
            {
                thresholds[row.ParameterKey] = row.ParameterValue;
            }
        }

        int level = 1;
        for (int lvl = 1; lvl < thresholds.Length; lvl++)
        {
            if (exp >= thresholds[lvl])
            {
                level = lvl;
            }
            else
            {
                break;
            }
        }

        // Cap EXP at the last threshold (max level cap)
        if (thresholds.Length > 0 && exp > thresholds[^1])
        {
            exp = thresholds[^1];
        }

        return (level, exp);
    }

    /// <summary>
    /// Evaluates a numerical function (linear, monomial, polynomial) from master data parameters.
    /// </summary>
    /// <summary>
    /// Evaluates a master data numerical function (LINEAR, MONOMIAL, POLYNOMIAL, etc.) used for cost and threshold calculations.
    /// </summary>
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

    /// <summary>
    /// Evaluates a monomial function: p[0] * (value - 1)^p[1].
    /// </summary>
    /// <summary>
    /// Computes a monomial function: coefficient * (value - 1) ^ exponent.
    /// </summary>
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

    /// <summary>
    /// Limit-breaks a costume using materials: deducts materials and gold, increments break count.
    /// </summary>
    /// <summary>
    /// Limit breaks a costume using materials, raising its max level cap. Capped at 4 total limit breaks.
    /// </summary>
    public override Task<LimitBreakResponse> LimitBreak(LimitBreakRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserCostume? costume = null;
        foreach (EntityIUserCostume c in userDb.EntityIUserCostume)
        {
            if (c.UserCostumeUuid == request.UserCostumeUuid)
            {
                costume = c;
                break;
            }
        }

        if (costume == null || costume.LimitBreakCount >= _gameConfig.CostumeLimitBreakAvailableCount)
        {
            return Task.FromResult(new LimitBreakResponse());
        }

        EntityMCostume? costumeMaster = null;
        foreach (EntityMCostume cm in _masterDb.EntityMCostume)
        {
            if (cm.CostumeId == costume.CostumeId)
            {
                costumeMaster = cm;
                break;
            }
        }

        if (costumeMaster == null)
        {
            return Task.FromResult(new LimitBreakResponse());
        }

        // Consume limit break materials
        int totalMaterialCount = 0;
        foreach (KeyValuePair<int, int> entry in request.Materials)
        {
            int materialId = entry.Key;
            int count = entry.Value;

            EntityIUserMaterial? userMat = FindUserMaterial(userDb, materialId);
            if (userMat == null)
            {
                continue;
            }

            if (userMat.Count < count)
            {
                count = userMat.Count;
            }

            userMat.Count -= count;
            totalMaterialCount += count;
        }

        // Deduct gold cost based on costume rarity
        if (totalMaterialCount > 0)
        {
            EntityMCostumeRarity? rarityRow = null;
            foreach (EntityMCostumeRarity r in _masterDb.EntityMCostumeRarity)
            {
                if (r.RarityType == costumeMaster.RarityType)
                {
                    rarityRow = r;
                    break;
                }
            }

            if (rarityRow != null)
            {
                int goldCost = EvaluateNumericalFunction(rarityRow.LimitBreakCostNumericalFunctionId, totalMaterialCount);
                SubtractGold(userDb, goldCost);
            }
        }

        costume.LimitBreakCount++;

        return Task.FromResult(new LimitBreakResponse());
    }

    /// <summary>
    /// Awakens a costume: deducts materials and gold, applies awaken effects (status up, item acquire).
    /// </summary>
    /// <summary>
    /// Awakens a costume to the next step, consuming materials and gold. Each step may grant stat bonuses, abilities, or items.
    /// </summary>
    public override Task<AwakenResponse> Awaken(AwakenRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserCostume? costume = null;
        foreach (EntityIUserCostume c in userDb.EntityIUserCostume)
        {
            if (c.UserCostumeUuid == request.UserCostumeUuid)
            {
                costume = c;
                break;
            }
        }

        if (costume == null)
        {
            return Task.FromResult(new AwakenResponse());
        }

        EntityMCostumeAwaken? awakenRow = null;
        foreach (EntityMCostumeAwaken a in _masterDb.EntityMCostumeAwaken)
        {
            if (a.CostumeId == costume.CostumeId)
            {
                awakenRow = a;
                break;
            }
        }

        if (awakenRow == null)
        {
            return Task.FromResult(new AwakenResponse());
        }

        int nextStep = costume.AwakenCount + 1;

        // Find gold cost from the price tier matching this awaken step
        int goldCost = 0;
        int bestStepLimit = -1;
        foreach (EntityMCostumeAwakenPriceGroup pg in _masterDb.EntityMCostumeAwakenPriceGroup)
        {
            if (pg.CostumeAwakenPriceGroupId == awakenRow.CostumeAwakenPriceGroupId
                && pg.AwakenStepLowerLimit <= nextStep
                && pg.AwakenStepLowerLimit > bestStepLimit)
            {
                bestStepLimit = pg.AwakenStepLowerLimit;
                goldCost = pg.Gold;
            }
        }

        if (goldCost > 0)
        {
            SubtractGold(userDb, goldCost);
        }

        // Consume awakening materials
        foreach (KeyValuePair<int, int> entry in request.Materials)
        {
            int materialId = entry.Key;
            int count = entry.Value;

            EntityIUserMaterial? userMat = FindUserMaterial(userDb, materialId);
            if (userMat == null)
            {
                continue;
            }

            if (userMat.Count < count)
            {
                count = userMat.Count;
            }

            userMat.Count -= count;
        }

        costume.AwakenCount = nextStep;

        // Apply the awaken step's effect (stat boost, ability unlock, or item grant)
        EntityMCostumeAwakenEffectGroup? effect = null;
        foreach (EntityMCostumeAwakenEffectGroup eg in _masterDb.EntityMCostumeAwakenEffectGroup)
        {
            if (eg.CostumeAwakenEffectGroupId == awakenRow.CostumeAwakenEffectGroupId
                && eg.AwakenStep == nextStep)
            {
                effect = eg;
                break;
            }
        }

        if (effect != null)
        {
            switch (effect.CostumeAwakenEffectType)
            {
                case CostumeAwakenEffectType.STATUS_UP:
                    ApplyAwakenStatusUp(userDb, request.UserCostumeUuid, effect.CostumeAwakenEffectId);
                    break;
                case CostumeAwakenEffectType.ABILITY:
                    break;
                case CostumeAwakenEffectType.ITEM_ACQUIRE:
                    ApplyAwakenItemAcquire(userDb, effect.CostumeAwakenEffectId);
                    break;
            }
        }

        return Task.FromResult(new AwakenResponse());
    }

    /// <summary>
    /// Levels up a costume's active skill: deducts materials and gold per level.
    /// </summary>
    /// <summary>
    /// Levels up a costume's active skill by spending materials and gold. Max skill level is determined by costume rarity.
    /// </summary>
    public override Task<EnhanceActiveSkillResponse> EnhanceActiveSkill(EnhanceActiveSkillRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserCostume? costume = null;
        foreach (EntityIUserCostume c in userDb.EntityIUserCostume)
        {
            if (c.UserCostumeUuid == request.UserCostumeUuid)
            {
                costume = c;
                break;
            }
        }

        if (costume == null)
        {
            return Task.FromResult(new EnhanceActiveSkillResponse());
        }

        EntityMCostume? costumeMaster = null;
        foreach (EntityMCostume cm in _masterDb.EntityMCostume)
        {
            if (cm.CostumeId == costume.CostumeId)
            {
                costumeMaster = cm;
                break;
            }
        }

        if (costumeMaster == null)
        {
            return Task.FromResult(new EnhanceActiveSkillResponse());
        }

        // Select the skill group tier unlocked by the costume's limit break count
        int enhanceMatId = -1;
        int bestLbThreshold = -1;
        foreach (EntityMCostumeActiveSkillGroup g in _masterDb.EntityMCostumeActiveSkillGroup)
        {
            if (g.CostumeActiveSkillGroupId == costumeMaster.CostumeActiveSkillGroupId
                && g.CostumeLimitBreakCountLowerLimit <= costume.LimitBreakCount
                && g.CostumeLimitBreakCountLowerLimit > bestLbThreshold)
            {
                bestLbThreshold = g.CostumeLimitBreakCountLowerLimit;
                enhanceMatId = g.CostumeActiveSkillEnhancementMaterialId;
            }
        }

        if (enhanceMatId < 0)
        {
            return Task.FromResult(new EnhanceActiveSkillResponse());
        }

        // Look up the user's current active skill level
        EntityIUserCostumeActiveSkill? skill = null;
        foreach (EntityIUserCostumeActiveSkill s in userDb.EntityIUserCostumeActiveSkill)
        {
            if (s.UserCostumeUuid == request.UserCostumeUuid)
            {
                skill = s;
                break;
            }
        }

        int currentLevel = skill?.Level ?? 0;

        // Determine max skill level from costume rarity
        EntityMCostumeRarity? rarityRow = null;
        foreach (EntityMCostumeRarity r in _masterDb.EntityMCostumeRarity)
        {
            if (r.RarityType == costumeMaster.RarityType)
            {
                rarityRow = r;
                break;
            }
        }

        if (rarityRow == null)
        {
            return Task.FromResult(new EnhanceActiveSkillResponse());
        }

        int maxLevel = EvaluateNumericalFunction(rarityRow.ActiveSkillMaxLevelNumericalFunctionId, 1);

        // Cap the requested level increase at the max
        int addCount = request.AddLevelCount;
        if (currentLevel + addCount > maxLevel)
        {
            addCount = maxLevel - currentLevel;
        }

        if (addCount <= 0)
        {
            return Task.FromResult(new EnhanceActiveSkillResponse());
        }

        // Deduct materials and gold for each level gained
        for (int lvl = currentLevel; lvl < currentLevel + addCount; lvl++)
        {
            foreach (EntityMCostumeActiveSkillEnhancementMaterial mat in _masterDb.EntityMCostumeActiveSkillEnhancementMaterial)
            {
                if (mat.CostumeActiveSkillEnhancementMaterialId == enhanceMatId && mat.SkillLevel == lvl)
                {
                    EntityIUserMaterial? userMat = FindUserMaterial(userDb, mat.MaterialId);
                    if (userMat != null)
                    {
                        int cost = mat.Count;
                        if (userMat.Count < cost)
                        {
                            cost = userMat.Count;
                        }
                        userMat.Count -= cost;
                    }
                }
            }

            int goldCost = EvaluateNumericalFunction(rarityRow.ActiveSkillEnhancementCostNumericalFunctionId, lvl + 1);
            SubtractGold(userDb, goldCost);
        }

        // Create the active skill record on first enhancement
        if (skill == null)
        {
            skill = new EntityIUserCostumeActiveSkill
            {
                UserId = userId,
                UserCostumeUuid = request.UserCostumeUuid,
                AcquisitionDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
            userDb.EntityIUserCostumeActiveSkill.Add(skill);
        }

        skill.Level = currentLevel + addCount;

        return Task.FromResult(new EnhanceActiveSkillResponse());
    }

    /// <summary>
    /// Stub for level bonus confirmation; returns empty response.
    /// </summary>
    /// <summary>
    /// Acknowledges that the player has seen the level bonus notification. No server-side state change needed.
    /// </summary>
    public override Task<RegisterLevelBonusConfirmedResponse> RegisterLevelBonusConfirmed(RegisterLevelBonusConfirmedRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RegisterLevelBonusConfirmedResponse());
    }

    /// <summary>
    /// Stub for lottery effect slot unlock; returns empty response.
    /// </summary>
    /// <summary>
    /// Unlocks a lottery effect slot on a costume. Not yet implemented.
    /// </summary>
    public override Task<UnlockLotteryEffectSlotResponse> UnlockLotteryEffectSlot(UnlockLotteryEffectSlotRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UnlockLotteryEffectSlotResponse());
    }

    /// <summary>
    /// Stub for lottery effect draw; returns empty response.
    /// </summary>
    /// <summary>
    /// Draws a random lottery effect for a costume slot. Not yet implemented.
    /// </summary>
    public override Task<DrawLotteryEffectResponse> DrawLotteryEffect(DrawLotteryEffectRequest request, ServerCallContext context)
    {
        return Task.FromResult(new DrawLotteryEffectResponse());
    }

    /// <summary>
    /// Stub for lottery effect confirmation; returns empty response.
    /// </summary>
    /// <summary>
    /// Confirms and locks in a drawn lottery effect for a costume. Not yet implemented.
    /// </summary>
    public override Task<ConfirmLotteryEffectResponse> ConfirmLotteryEffect(ConfirmLotteryEffectRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ConfirmLotteryEffectResponse());
    }

    /// <summary>
    /// Applies stat increases from a costume awaken status-up effect.
    /// </summary>
    /// <summary>
    /// Applies awakening stat bonuses (HP, ATK, VIT, AGI, CRIT, etc.) to the costume's awaken status record.
    /// </summary>
    private void ApplyAwakenStatusUp(DarkUserMemoryDatabase userDb, string userCostumeUuid, int statusUpGroupId)
    {
        foreach (EntityMCostumeAwakenStatusUpGroup row in _masterDb.EntityMCostumeAwakenStatusUpGroup)
        {
            if (row.CostumeAwakenStatusUpGroupId != statusUpGroupId)
            {
                continue;
            }

            EntityIUserCostumeAwakenStatusUp? state = null;
            foreach (EntityIUserCostumeAwakenStatusUp s in userDb.EntityIUserCostumeAwakenStatusUp)
            {
                if (s.UserCostumeUuid == userCostumeUuid && s.StatusCalculationType == row.StatusCalculationType)
                {
                    state = s;
                    break;
                }
            }

            if (state == null)
            {
                state = new EntityIUserCostumeAwakenStatusUp
                {
                    UserId = userDb.UserId,
                    UserCostumeUuid = userCostumeUuid,
                    StatusCalculationType = row.StatusCalculationType,
                };
                userDb.EntityIUserCostumeAwakenStatusUp.Add(state);
            }

            switch (row.StatusKindType)
            {
                case StatusKindType.HP:
                    state.Hp += row.EffectValue;
                    break;
                case StatusKindType.ATTACK:
                    state.Attack += row.EffectValue;
                    break;
                case StatusKindType.VITALITY:
                    state.Vitality += row.EffectValue;
                    break;
                case StatusKindType.AGILITY:
                    state.Agility += row.EffectValue;
                    break;
                case StatusKindType.CRITICAL_RATIO:
                    state.CriticalRatio += row.EffectValue;
                    break;
                case StatusKindType.CRITICAL_ATTACK:
                    state.CriticalAttack += row.EffectValue;
                    break;
            }
        }
    }

    /// <summary>
    /// Grants a thought item from a costume awaken item-acquire effect.
    /// </summary>
    /// <summary>
    /// Grants a thought item as an awakening reward, creating a new inventory entry if not already owned.
    /// </summary>
    private void ApplyAwakenItemAcquire(DarkUserMemoryDatabase userDb, int itemAcquireId)
    {
        EntityMCostumeAwakenItemAcquire? acq = null;
        foreach (EntityMCostumeAwakenItemAcquire a in _masterDb.EntityMCostumeAwakenItemAcquire)
        {
            if (a.CostumeAwakenItemAcquireId == itemAcquireId)
            {
                acq = a;
                break;
            }
        }

        if (acq == null)
        {
            return;
        }

        string uuid = $"awaken-thought-{acq.PossessionId}";
        foreach (EntityIUserThought t in userDb.EntityIUserThought)
        {
            if (t.UserThoughtUuid == uuid)
            {
                return;
            }
        }

        userDb.EntityIUserThought.Add(new EntityIUserThought
        {
            UserId = userDb.UserId,
            UserThoughtUuid = uuid,
            ThoughtId = acq.PossessionId,
            AcquisitionDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        });
    }

    /// <summary>
    /// Finds a user's material record by material ID.
    /// </summary>
    /// <summary>
    /// Looks up a user's material inventory entry by material ID.
    /// </summary>
    private static EntityIUserMaterial? FindUserMaterial(DarkUserMemoryDatabase userDb, int materialId)
    {
        foreach (EntityIUserMaterial m in userDb.EntityIUserMaterial)
        {
            if (m.MaterialId == materialId)
            {
                return m;
            }
        }
        return null;
    }

    /// <summary>
    /// Deducts gold (consumable item ID 1) from the user's inventory.
    /// </summary>
    /// <summary>
    /// Deducts gold (consumable item ID 1) from the user's inventory.
    /// </summary>
    private void SubtractGold(DarkUserMemoryDatabase userDb, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        foreach (EntityIUserConsumableItem ci in userDb.EntityIUserConsumableItem)
        {
            if (ci.ConsumableItemId == _gameConfig.ConsumableItemIdForGold)
            {
                ci.Count -= amount;
                return;
            }
        }
    }
}
