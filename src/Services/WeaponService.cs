using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Weapon;

namespace MariesWonderland.Services;

public class WeaponService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.Weapon.WeaponService.WeaponServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>
    /// Sells one or more weapons from the player's inventory, granting gold based on weapon level and any exchange medals.
    /// </summary>
    /// <summary>
    /// Sells weapons: grants gold based on level, awards exchange items, and removes weapon records.
    /// </summary>
    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int totalGold = 0;

        foreach (string uuid in request.UserWeaponUuid)
        {
            EntityIUserWeapon? weapon = FindWeapon(userDb, uuid);
            if (weapon == null)
            {
                continue;
            }

            EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
            if (wm == null)
            {
                continue;
            }

            // Calculate sell price based on weapon level
            EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
            if (enhance != null)
            {
                totalGold += EvaluateNumericalFunction(enhance.SellPriceNumericalFunctionId, weapon.Level);
            }

            // Grant exchange medals associated with this weapon
            foreach (EntityMWeaponConsumeExchangeConsumableItemGroup exchange in _masterDb.EntityMWeaponConsumeExchangeConsumableItemGroup)
            {
                if (exchange.WeaponId == weapon.WeaponId)
                {
                    AddConsumableItem(userDb, exchange.ConsumableItemId, exchange.Count);
                }
            }

            // Remove the weapon and all associated skill/ability/awaken records
            userDb.EntityIUserWeapon.Remove(weapon);
            userDb.EntityIUserWeaponSkill.RemoveAll(s => s.UserWeaponUuid == uuid);
            userDb.EntityIUserWeaponAbility.RemoveAll(a => a.UserWeaponUuid == uuid);
            userDb.EntityIUserWeaponAwaken.RemoveAll(a => a.UserWeaponUuid == uuid);
        }

        if (totalGold > 0)
        {
            AddConsumableItem(userDb, _gameConfig.ConsumableItemIdForGold, totalGold);
        }

        return Task.FromResult(new SellResponse());
    }

    /// <summary>
    /// Marks one or more weapons as protected, preventing them from being accidentally sold or used as fodder.
    /// </summary>
    /// <summary>
    /// Marks weapons as protected to prevent accidental sale or consumption.
    /// </summary>
    public override Task<ProtectResponse> Protect(ProtectRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (string uuid in request.UserWeaponUuid)
        {
            EntityIUserWeapon? weapon = FindWeapon(userDb, uuid);
            if (weapon != null)
            {
                weapon.IsProtected = true;
            }
        }

        return Task.FromResult(new ProtectResponse());
    }

    /// <summary>
    /// Removes the protection flag from one or more weapons, allowing them to be sold or used as fodder again.
    /// </summary>
    /// <summary>
    /// Removes protection from weapons, allowing sale or consumption.
    /// </summary>
    public override Task<UnprotectResponse> Unprotect(UnprotectRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (string uuid in request.UserWeaponUuid)
        {
            EntityIUserWeapon? weapon = FindWeapon(userDb, uuid);
            if (weapon != null)
            {
                weapon.IsProtected = false;
            }
        }

        return Task.FromResult(new UnprotectResponse());
    }

    /// <summary>
    /// Enhances a weapon using enhancement materials to gain EXP. Materials matching the weapon's type grant a 1.5x EXP bonus.
    /// </summary>
    public override Task<EnhanceByMaterialResponse> EnhanceByMaterial(EnhanceByMaterialRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null)
        {
            return Task.FromResult(new EnhanceByMaterialResponse { IsGreatSuccess = false });
        }

        EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
        if (wm == null)
        {
            return Task.FromResult(new EnhanceByMaterialResponse { IsGreatSuccess = false });
        }

        // Consume materials and calculate total EXP gained
        int totalExp = 0;
        int totalMaterialCount = 0;

        foreach (KeyValuePair<int, int> entry in request.Materials)
        {
            int materialId = entry.Key;
            int count = entry.Value;

            EntityMMaterial? mat = FindMaterial(materialId);
            if (mat == null)
            {
                continue;
            }

            EntityIUserMaterial? userMat = FindUserMaterial(userDb, materialId);
            if (userMat == null || userMat.Count < count)
            {
                continue;
            }

            userMat.Count -= count;
            totalMaterialCount += count;

            // Apply 1.5x EXP bonus when material weapon type matches the target weapon
            int expPerUnit = mat.EffectValue;
            if (mat.WeaponType != WeaponType.UNKNOWN && mat.WeaponType == wm.WeaponType)
            {
                expPerUnit = expPerUnit * _gameConfig.MaterialSameWeaponExpCoefficientPermil / 1000;
            }
            totalExp += expPerUnit * count;
        }

        // Deduct gold cost scaled by number of materials used
        if (totalMaterialCount > 0)
        {
            EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
            if (enhance != null)
            {
                int goldCost = EvaluateNumericalFunction(enhance.EnhancementCostByMaterialNumericalFunctionId, totalMaterialCount);
                SubtractGold(userDb, goldCost);
            }
        }

        // Apply EXP, recalculate level, and check for new story page unlocks
        weapon.Exp += totalExp;
        ApplyLevelFromExp(weapon, wm);

        CheckWeaponStoryUnlocks(userDb, weapon.WeaponId, weapon.Level);

        return Task.FromResult(new EnhanceByMaterialResponse{ IsGreatSuccess = false });
    }

    /// <summary>
    /// Enhances a weapon by consuming other weapons as EXP fodder. Same-type fodder grants a 1.5x EXP bonus.
    /// </summary>
    public override Task<EnhanceByWeaponResponse> EnhanceByWeapon(EnhanceByWeaponRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null)
        {
            return Task.FromResult(new EnhanceByWeaponResponse { IsGreatSuccess = false });
        }

        EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
        if (wm == null)
        {
            return Task.FromResult(new EnhanceByWeaponResponse { IsGreatSuccess = false });
        }

        int totalExp = 0;
        int consumedCount = 0;

        foreach (string matUuid in request.MaterialUserWeaponUuids)
        {
            EntityIUserWeapon? matWeapon = FindWeapon(userDb, matUuid);
            if (matWeapon == null)
            {
                continue;
            }

            // Calculate EXP from the fodder weapon, with same-type bonus
            EntityMWeapon? matMaster = FindWeaponMaster(matWeapon.WeaponId);
            if (matMaster == null)
            {
                continue;
            }

            EntityMWeaponSpecificEnhance? matEnhance = FindEnhance(matMaster.WeaponSpecificEnhanceId);
            int baseExp = matEnhance?.BaseEnhancementObtainedExp ?? 0;
            if (matMaster.WeaponType != WeaponType.UNKNOWN && matMaster.WeaponType == wm.WeaponType)
            {
                baseExp = baseExp * _gameConfig.MaterialSameWeaponExpCoefficientPermil / 1000;
            }
            totalExp += baseExp;

            // Grant exchange medals before destroying the fodder weapon
            foreach (EntityMWeaponConsumeExchangeConsumableItemGroup exchange in _masterDb.EntityMWeaponConsumeExchangeConsumableItemGroup)
            {
                if (exchange.WeaponId == matWeapon.WeaponId)
                {
                    AddConsumableItem(userDb, exchange.ConsumableItemId, exchange.Count);
                }
            }

            // Remove the consumed fodder weapon and all its associated records
            userDb.EntityIUserWeapon.Remove(matWeapon);
            userDb.EntityIUserWeaponSkill.RemoveAll(s => s.UserWeaponUuid == matUuid);
            userDb.EntityIUserWeaponAbility.RemoveAll(a => a.UserWeaponUuid == matUuid);
            userDb.EntityIUserWeaponAwaken.RemoveAll(a => a.UserWeaponUuid == matUuid);
            consumedCount++;
        }

        // Deduct gold cost scaled by number of weapons consumed
        if (consumedCount > 0)
        {
            EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
            if (enhance != null)
            {
                int goldCost = EvaluateNumericalFunction(enhance.EnhancementCostByWeaponNumericalFunctionId, consumedCount);
                SubtractGold(userDb, goldCost);
            }
        }

        // Apply EXP, recalculate level, and check for new story page unlocks
        weapon.Exp += totalExp;
        ApplyLevelFromExp(weapon, wm);

        CheckWeaponStoryUnlocks(userDb, weapon.WeaponId, weapon.Level);

        return Task.FromResult(new EnhanceByWeaponResponse{ IsGreatSuccess = false });
    }

    /// <summary>
    /// Levels up a weapon's skill by spending materials and gold. Max skill level depends on the weapon's limit break count.
    /// </summary>
    public override Task<EnhanceSkillResponse> EnhanceSkill(EnhanceSkillRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null)
        {
            return Task.FromResult(new EnhanceSkillResponse());
        }

        EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
        if (wm == null)
        {
            return Task.FromResult(new EnhanceSkillResponse());
        }

        // Look up which skill slot corresponds to the requested skill ID
        EntityMWeaponSkillGroup? skillGroup = null;
        foreach (EntityMWeaponSkillGroup sg in _masterDb.EntityMWeaponSkillGroup)
        {
            if (sg.WeaponSkillGroupId == wm.WeaponSkillGroupId && sg.SkillId == request.SkillId)
            {
                skillGroup = sg;
                break;
            }
        }

        if (skillGroup == null)
        {
            return Task.FromResult(new EnhanceSkillResponse());
        }

        // Find the user's current skill level for this slot
        EntityIUserWeaponSkill? userSkill = null;
        foreach (EntityIUserWeaponSkill s in userDb.EntityIUserWeaponSkill)
        {
            if (s.UserWeaponUuid == request.UserWeaponUuid && s.SlotNumber == skillGroup.SlotNumber)
            {
                userSkill = s;
                break;
            }
        }

        if (userSkill == null)
        {
            return Task.FromResult(new EnhanceSkillResponse());
        }

        // Cap the requested level increase at the skill's max level (based on limit break count)
        EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
        if (enhance == null)
        {
            return Task.FromResult(new EnhanceSkillResponse());
        }

        int maxLevel = EvaluateNumericalFunction(enhance.MaxSkillLevelNumericalFunctionId, weapon.LimitBreakCount);
        int addCount = request.AddLevelCount;
        if (userSkill.Level + addCount > maxLevel)
        {
            addCount = maxLevel - userSkill.Level;
        }
        if (addCount <= 0)
        {
            return Task.FromResult(new EnhanceSkillResponse());
        }

        // Deduct materials and gold for each level gained
        for (int lvl = userSkill.Level; lvl < userSkill.Level + addCount; lvl++)
        {
            foreach (EntityMWeaponSkillEnhancementMaterial mat in _masterDb.EntityMWeaponSkillEnhancementMaterial)
            {
                if (mat.WeaponSkillEnhancementMaterialId == skillGroup.WeaponSkillEnhancementMaterialId && mat.SkillLevel == lvl)
                {
                    EntityIUserMaterial? userMat = FindUserMaterial(userDb, mat.MaterialId);
                    if (userMat != null)
                    {
                        int cost = Math.Min(mat.Count, userMat.Count);
                        userMat.Count -= cost;
                    }
                }
            }

            int goldCost = EvaluateNumericalFunction(enhance.SkillEnhancementCostNumericalFunctionId, lvl + 1);
            SubtractGold(userDb, goldCost);
        }

        userSkill.Level += addCount;

        return Task.FromResult(new EnhanceSkillResponse());
    }

    /// <summary>
    /// Levels up a weapon's passive ability by spending materials and gold. Max ability level depends on the weapon's limit break count.
    /// </summary>
    public override Task<EnhanceAbilityResponse> EnhanceAbility(EnhanceAbilityRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null)
        {
            return Task.FromResult(new EnhanceAbilityResponse());
        }

        EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
        if (wm == null)
        {
            return Task.FromResult(new EnhanceAbilityResponse());
        }

        // Look up which ability slot corresponds to the requested ability ID
        EntityMWeaponAbilityGroup? abilityGroup = null;
        foreach (EntityMWeaponAbilityGroup ag in _masterDb.EntityMWeaponAbilityGroup)
        {
            if (ag.WeaponAbilityGroupId == wm.WeaponAbilityGroupId && ag.AbilityId == request.AbilityId)
            {
                abilityGroup = ag;
                break;
            }
        }

        if (abilityGroup == null)
        {
            return Task.FromResult(new EnhanceAbilityResponse());
        }

        // Find the user's current ability level for this slot
        EntityIUserWeaponAbility? userAbility = null;
        foreach (EntityIUserWeaponAbility a in userDb.EntityIUserWeaponAbility)
        {
            if (a.UserWeaponUuid == request.UserWeaponUuid && a.SlotNumber == abilityGroup.SlotNumber)
            {
                userAbility = a;
                break;
            }
        }

        if (userAbility == null)
        {
            return Task.FromResult(new EnhanceAbilityResponse());
        }

        // Cap the requested level increase at the ability's max level (based on limit break count)
        EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
        if (enhance == null)
        {
            return Task.FromResult(new EnhanceAbilityResponse());
        }

        int maxLevel = EvaluateNumericalFunction(enhance.MaxAbilityLevelNumericalFunctionId, weapon.LimitBreakCount);
        int addCount = request.AddLevelCount;
        if (userAbility.Level + addCount > maxLevel)
        {
            addCount = maxLevel - userAbility.Level;
        }
        if (addCount <= 0)
        {
            return Task.FromResult(new EnhanceAbilityResponse());
        }

        // Deduct materials and gold for each level gained
        for (int lvl = userAbility.Level; lvl < userAbility.Level + addCount; lvl++)
        {
            foreach (EntityMWeaponAbilityEnhancementMaterial mat in _masterDb.EntityMWeaponAbilityEnhancementMaterial)
            {
                if (mat.WeaponAbilityEnhancementMaterialId == abilityGroup.WeaponAbilityEnhancementMaterialId && mat.AbilityLevel == lvl)
                {
                    EntityIUserMaterial? userMat = FindUserMaterial(userDb, mat.MaterialId);
                    if (userMat != null)
                    {
                        int cost = Math.Min(mat.Count, userMat.Count);
                        userMat.Count -= cost;
                    }
                }
            }

            int goldCost = EvaluateNumericalFunction(enhance.AbilityEnhancementCostNumericalFunctionId, lvl + 1);
            SubtractGold(userDb, goldCost);
        }

        userAbility.Level += addCount;

        return Task.FromResult(new EnhanceAbilityResponse());
    }

    /// <summary>
    /// Limit-breaks a weapon using materials: deducts materials and gold, increments break count.
    /// </summary>
    /// <summary>
    /// Limit breaks a weapon using materials, raising its max level cap. Capped at 4 total limit breaks.
    /// </summary>
    public override Task<LimitBreakByMaterialResponse> LimitBreakByMaterial(LimitBreakByMaterialRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null || weapon.LimitBreakCount >= _gameConfig.WeaponLimitBreakAvailableCount)
        {
            return Task.FromResult(new LimitBreakByMaterialResponse());
        }

        EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
        if (wm == null)
        {
            return Task.FromResult(new LimitBreakByMaterialResponse());
        }

        // Consume materials up to the remaining limit break slots
        int remaining = _gameConfig.WeaponLimitBreakAvailableCount - weapon.LimitBreakCount;
        int totalMaterialCount = 0;

        foreach (KeyValuePair<int, int> entry in request.Materials)
        {
            if (totalMaterialCount >= remaining)
            {
                break;
            }

            int materialId = entry.Key;
            int count = entry.Value;
            if (count > remaining - totalMaterialCount)
            {
                count = remaining - totalMaterialCount;
            }

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

        // Deduct gold cost for the limit break
        if (totalMaterialCount > 0)
        {
            EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
            if (enhance != null)
            {
                int goldCost = EvaluateNumericalFunction(enhance.LimitBreakCostByMaterialNumericalFunctionId, totalMaterialCount);
                SubtractGold(userDb, goldCost);
            }
        }

        weapon.LimitBreakCount += totalMaterialCount;

        // Track the highest limit break count in the weapon note
        UpdateWeaponNote(userDb, weapon);

        return Task.FromResult(new LimitBreakByMaterialResponse());
    }

    /// <summary>
    /// Limit-breaks a weapon by consuming duplicate weapons: awards exchange items, deducts gold.
    /// </summary>
    /// <summary>
    /// Limit breaks a weapon by consuming duplicate weapons, raising its max level cap. Capped at 4 total limit breaks.
    /// </summary>
    public override Task<LimitBreakByWeaponResponse> LimitBreakByWeapon(LimitBreakByWeaponRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null || weapon.LimitBreakCount >= _gameConfig.WeaponLimitBreakAvailableCount)
        {
            return Task.FromResult(new LimitBreakByWeaponResponse());
        }

        EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
        if (wm == null)
        {
            return Task.FromResult(new LimitBreakByWeaponResponse());
        }

        // Consume duplicate weapons up to the remaining limit break slots
        int remaining = _gameConfig.WeaponLimitBreakAvailableCount - weapon.LimitBreakCount;
        int consumedCount = 0;

        foreach (string matUuid in request.MaterialUserWeaponUuids)
        {
            if (consumedCount >= remaining)
            {
                break;
            }

            EntityIUserWeapon? matWeapon = FindWeapon(userDb, matUuid);
            if (matWeapon == null)
            {
                continue;
            }

            // Grant exchange medals before destroying the consumed weapon
            foreach (EntityMWeaponConsumeExchangeConsumableItemGroup exchange in _masterDb.EntityMWeaponConsumeExchangeConsumableItemGroup)
            {
                if (exchange.WeaponId == matWeapon.WeaponId)
                {
                    AddConsumableItem(userDb, exchange.ConsumableItemId, exchange.Count);
                }
            }

            // Remove the consumed weapon and all its associated records
            userDb.EntityIUserWeapon.Remove(matWeapon);
            userDb.EntityIUserWeaponSkill.RemoveAll(s => s.UserWeaponUuid == matUuid);
            userDb.EntityIUserWeaponAbility.RemoveAll(a => a.UserWeaponUuid == matUuid);
            userDb.EntityIUserWeaponAwaken.RemoveAll(a => a.UserWeaponUuid == matUuid);
            consumedCount++;
        }

        // Deduct gold cost for the limit break
        if (consumedCount > 0)
        {
            EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
            if (enhance != null)
            {
                int goldCost = EvaluateNumericalFunction(enhance.LimitBreakCostByWeaponNumericalFunctionId, consumedCount);
                SubtractGold(userDb, goldCost);
            }
        }

        weapon.LimitBreakCount += consumedCount;

        // Track the highest limit break count in the weapon note
        UpdateWeaponNote(userDb, weapon);

        return Task.FromResult(new LimitBreakByWeaponResponse());
    }

    /// <summary>
    /// Evolves a weapon to its next form: deducts materials and gold, updates weapon ID and ability slots.
    /// </summary>
    /// <summary>
    /// Evolves a weapon to its next form, consuming evolution materials and gold. Resets ability slots to the evolved weapon's layout.
    /// </summary>
    public override Task<EvolveResponse> Evolve(EvolveRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null)
        {
            return Task.FromResult(new EvolveResponse());
        }

        EntityMWeapon? wm = FindWeaponMaster(weapon.WeaponId);
        if (wm == null)
        {
            return Task.FromResult(new EvolveResponse());
        }

        // Find the next weapon in the evolution chain
        int evolvedId = 0;
        foreach (EntityMWeaponEvolutionGroup evo in _masterDb.EntityMWeaponEvolutionGroup)
        {
            if (evo.WeaponId == weapon.WeaponId)
            {
                int currentOrder = evo.EvolutionOrder;
                foreach (EntityMWeaponEvolutionGroup next in _masterDb.EntityMWeaponEvolutionGroup)
                {
                    if (next.WeaponEvolutionGroupId == evo.WeaponEvolutionGroupId && next.EvolutionOrder == currentOrder + 1)
                    {
                        evolvedId = next.WeaponId;
                        break;
                    }
                }
                break;
            }
        }

        if (evolvedId == 0)
        {
            return Task.FromResult(new EvolveResponse());
        }

        // Consume evolution materials
        int totalMaterialCount = 0;
        foreach (EntityMWeaponEvolutionMaterialGroup mat in _masterDb.EntityMWeaponEvolutionMaterialGroup)
        {
            if (mat.WeaponEvolutionMaterialGroupId == wm.WeaponEvolutionMaterialGroupId)
            {
                EntityIUserMaterial? userMat = FindUserMaterial(userDb, mat.MaterialId);
                if (userMat != null)
                {
                    int cost = Math.Min(mat.Count, userMat.Count);
                    userMat.Count -= cost;
                    totalMaterialCount += cost;
                }
            }
        }

        // Deduct gold cost for the evolution
        if (totalMaterialCount > 0)
        {
            EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
            if (enhance != null)
            {
                int goldCost = EvaluateNumericalFunction(enhance.EvolutionCostNumericalFunctionId, totalMaterialCount);
                SubtractGold(userDb, goldCost);
            }
        }

        // Transform the weapon into its evolved form
        weapon.WeaponId = evolvedId;

        // Replace ability slots with the evolved weapon's ability layout
        EntityMWeapon? evolvedMaster = FindWeaponMaster(evolvedId);
        if (evolvedMaster != null)
        {
            userDb.EntityIUserWeaponAbility.RemoveAll(a => a.UserWeaponUuid == request.UserWeaponUuid);
            foreach (EntityMWeaponAbilityGroup ag in _masterDb.EntityMWeaponAbilityGroup)
            {
                if (ag.WeaponAbilityGroupId == evolvedMaster.WeaponAbilityGroupId)
                {
                    userDb.EntityIUserWeaponAbility.Add(new EntityIUserWeaponAbility
                    {
                        UserId = userId,
                        UserWeaponUuid = request.UserWeaponUuid,
                        SlotNumber = ag.SlotNumber,
                        Level = 1
                    });
                }
            }
        }

        // Check if the evolution unlocks new weapon story pages
        CheckWeaponStoryUnlocks(userDb, evolvedId, weapon.Level);

        return Task.FromResult(new EvolveResponse());
    }

    /// <summary>
    /// Awakens a weapon: deducts materials and gold, records awaken state.
    /// </summary>
    /// <summary>
    /// Awakens a weapon, unlocking its awakening bonus. Consumes awakening materials and gold. Can only be done once per weapon.
    /// </summary>
    public override Task<AwakenResponse> Awaken(AwakenRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserWeapon? weapon = FindWeapon(userDb, request.UserWeaponUuid);
        if (weapon == null)
        {
            return Task.FromResult(new AwakenResponse());
        }

        // Weapon can only be awakened once
        foreach (EntityIUserWeaponAwaken aw in userDb.EntityIUserWeaponAwaken)
        {
            if (aw.UserWeaponUuid == request.UserWeaponUuid)
            {
                return Task.FromResult(new AwakenResponse());
            }
        }

        // Look up awakening requirements from master data
        EntityMWeaponAwaken? awakenMaster = null;
        foreach (EntityMWeaponAwaken wa in _masterDb.EntityMWeaponAwaken)
        {
            if (wa.WeaponId == weapon.WeaponId)
            {
                awakenMaster = wa;
                break;
            }
        }

        if (awakenMaster == null)
        {
            return Task.FromResult(new AwakenResponse());
        }

        // Consume awakening materials
        foreach (EntityMWeaponAwakenMaterialGroup mat in _masterDb.EntityMWeaponAwakenMaterialGroup)
        {
            if (mat.WeaponAwakenMaterialGroupId == awakenMaster.WeaponAwakenMaterialGroupId)
            {
                EntityIUserMaterial? userMat = FindUserMaterial(userDb, mat.MaterialId);
                if (userMat != null)
                {
                    int cost = Math.Min(mat.Count, userMat.Count);
                    userMat.Count -= cost;
                }
            }
        }

        SubtractGold(userDb, awakenMaster.ConsumeGold);

        // Record the awakening
        userDb.EntityIUserWeaponAwaken.Add(new EntityIUserWeaponAwaken
        {
            UserId = userId,
            UserWeaponUuid = request.UserWeaponUuid
        });

        return Task.FromResult(new AwakenResponse());
    }

    #region Helper Methods

    /// <summary>
    /// Finds a user weapon by UUID.
    /// </summary>
    /// <summary>
    /// Looks up a user's weapon by its unique identifier.
    /// </summary>
    private static EntityIUserWeapon? FindWeapon(DarkUserMemoryDatabase userDb, string uuid)
    {
        foreach (EntityIUserWeapon w in userDb.EntityIUserWeapon)
        {
            if (w.UserWeaponUuid == uuid)
            {
                return w;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds a weapon's master data definition by weapon ID.
    /// </summary>
    /// <summary>
    /// Looks up weapon master data (read-only definition) by weapon ID.
    /// </summary>
    private EntityMWeapon? FindWeaponMaster(int weaponId)
    {
        foreach (EntityMWeapon w in _masterDb.EntityMWeapon)
        {
            if (w.WeaponId == weaponId)
            {
                return w;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds weapon-specific enhancement parameters by enhance ID.
    /// </summary>
    /// <summary>
    /// Looks up the weapon-specific enhancement parameters (cost formulas, max levels, etc.) by enhance ID.
    /// </summary>
    private EntityMWeaponSpecificEnhance? FindEnhance(int enhanceId)
    {
        foreach (EntityMWeaponSpecificEnhance e in _masterDb.EntityMWeaponSpecificEnhance)
        {
            if (e.WeaponSpecificEnhanceId == enhanceId)
            {
                return e;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds a material's master data definition by material ID.
    /// </summary>
    /// <summary>
    /// Looks up material master data (read-only definition) by material ID.
    /// </summary>
    private EntityMMaterial? FindMaterial(int materialId)
    {
        foreach (EntityMMaterial m in _masterDb.EntityMMaterial)
        {
            if (m.MaterialId == materialId)
            {
                return m;
            }
        }
        return null;
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
    /// Adds a consumable item to the user, creating the record if needed.
    /// </summary>
    /// <summary>
    /// Grants a consumable item to the user, creating the inventory entry if it doesn't exist yet.
    /// </summary>
    private static void AddConsumableItem(DarkUserMemoryDatabase userDb, int itemId, int count)
    {
        foreach (EntityIUserConsumableItem ci in userDb.EntityIUserConsumableItem)
        {
            if (ci.ConsumableItemId == itemId)
            {
                ci.Count += count;
                return;
            }
        }

        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = userDb.UserId,
            ConsumableItemId = itemId,
            Count = count,
            FirstAcquisitionDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
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

    /// <summary>
    /// Recalculates weapon level from current EXP using parameter map thresholds.
    /// </summary>
    /// <summary>
    /// Recalculates the weapon's level from its accumulated EXP using master data EXP thresholds. Caps EXP at the max threshold.
    /// </summary>
    private void ApplyLevelFromExp(EntityIUserWeapon weapon, EntityMWeapon wm)
    {
        EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
        if (enhance == null)
        {
            return;
        }

        // Build EXP-to-level threshold table from master data parameter map
        int paramMapId = enhance.RequiredExpForLevelUpNumericalParameterMapId;
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

        // Find the highest level whose cumulative EXP threshold is met
        int level = 1;
        for (int lvl = 1; lvl < thresholds.Length; lvl++)
        {
            if (weapon.Exp >= thresholds[lvl])
            {
                level = lvl;
            }
            else
            {
                break;
            }
        }

        // Cap EXP at the highest threshold when at max level
        if (level > 0 && level < thresholds.Length - 1)
        {
        }
        else if (level >= thresholds.Length - 1 && thresholds.Length > 1)
        {
            weapon.Exp = thresholds[^1];
        }

        weapon.Level = level;
    }

    /// <summary>
    /// Updates the weapon note's max limit break count if the weapon exceeds it.
    /// </summary>
    /// <summary>
    /// Updates the weapon note to track the highest limit break count ever achieved for this weapon.
    /// </summary>
    private static void UpdateWeaponNote(DarkUserMemoryDatabase userDb, EntityIUserWeapon weapon)
    {
        EntityIUserWeaponNote? note = null;
        foreach (EntityIUserWeaponNote n in userDb.EntityIUserWeaponNote)
        {
            if (n.WeaponId == weapon.WeaponId)
            {
                note = n;
                break;
            }
        }

        if (note != null && note.MaxLimitBreakCount < weapon.LimitBreakCount)
        {
            note.MaxLimitBreakCount = weapon.LimitBreakCount;
        }
    }

    /// <summary>
    /// Evaluates weapon story release conditions and unlocks eligible stories.
    /// </summary>
    /// <summary>
    /// Evaluates weapon story release conditions and unlocks story pages when conditions are met (level reached, evolution, acquisition, etc.).
    /// </summary>
    private void CheckWeaponStoryUnlocks(DarkUserMemoryDatabase userDb, int weaponId, int level)
    {
        EntityMWeapon? wm = FindWeaponMaster(weaponId);
        if (wm == null || wm.WeaponStoryReleaseConditionGroupId == 0)
        {
            return;
        }

        // Determine the weapon's position in its evolution chain
        bool hasEvo = false;
        int evoOrder = 0;
        foreach (EntityMWeaponEvolutionGroup evo in _masterDb.EntityMWeaponEvolutionGroup)
        {
            if (evo.WeaponId == weaponId)
            {
                hasEvo = true;
                evoOrder = evo.EvolutionOrder;
                break;
            }
        }

        // Evaluate each story release condition for this weapon
        foreach (EntityMWeaponStoryReleaseConditionGroup cond in _masterDb.EntityMWeaponStoryReleaseConditionGroup)
        {
            if (cond.WeaponStoryReleaseConditionGroupId != wm.WeaponStoryReleaseConditionGroupId)
            {
                continue;
            }

            bool shouldUnlock = cond.WeaponStoryReleaseConditionType switch
            {
                WeaponStoryReleaseConditionType.ACQUISITION => true,
                WeaponStoryReleaseConditionType.REACH_SPECIFIED_LEVEL => level >= cond.ConditionValue,
                WeaponStoryReleaseConditionType.REACH_INITIAL_MAX_LEVEL => CheckInitialMaxLevel(wm, level),
                WeaponStoryReleaseConditionType.REACH_ONCE_EVOLVED_MAX_LEVEL => hasEvo && evoOrder >= 1 && CheckInitialMaxLevel(wm, level),
                WeaponStoryReleaseConditionType.REACH_SPECIFIED_EVOLUTION_COUNT => hasEvo && evoOrder >= cond.ConditionValue,
                _ => false
            };

            if (shouldUnlock)
            {
                GrantWeaponStoryUnlock(userDb, weaponId, cond.StoryIndex);
            }
        }
    }

    /// <summary>
    /// Checks whether the weapon has reached its initial (pre-evolution) max level.
    /// </summary>
    /// <summary>
    /// Checks whether the weapon has reached its initial (pre-limit-break) max level.
    /// </summary>
    private bool CheckInitialMaxLevel(EntityMWeapon wm, int level)
    {
        EntityMWeaponSpecificEnhance? enhance = FindEnhance(wm.WeaponSpecificEnhanceId);
        if (enhance == null)
        {
            return false;
        }

        int maxLevel = EvaluateNumericalFunction(enhance.MaxLevelNumericalFunctionId, 0);
        return level >= maxLevel;
    }

    /// <summary>
    /// Creates or updates a weapon story unlock record.
    /// </summary>
    /// <summary>
    /// Unlocks a weapon story page, creating the story record if new or advancing the max released story index.
    /// </summary>
    private static void GrantWeaponStoryUnlock(DarkUserMemoryDatabase userDb, int weaponId, int storyIndex)
    {
        EntityIUserWeaponStory? existing = null;
        foreach (EntityIUserWeaponStory ws in userDb.EntityIUserWeaponStory)
        {
            if (ws.WeaponId == weaponId)
            {
                existing = ws;
                break;
            }
        }

        if (existing != null)
        {
            if (existing.ReleasedMaxStoryIndex < storyIndex)
            {
                existing.ReleasedMaxStoryIndex = storyIndex;
            }
        }
        else
        {
            userDb.EntityIUserWeaponStory.Add(new EntityIUserWeaponStory
            {
                UserId = userDb.UserId,
                WeaponId = weaponId,
                ReleasedMaxStoryIndex = storyIndex
            });
        }
    }

    /// <summary>
    /// Evaluates a numerical function (linear, monomial, polynomial) from master data parameters.
    /// </summary>
    /// <summary>
    /// Evaluates a master data numerical function (LINEAR, MONOMIAL, POLYNOMIAL, etc.) used for cost and threshold calculations.
    /// </summary>
    private int EvaluateNumericalFunction(int functionId, int value)    {
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

    #endregion
}
