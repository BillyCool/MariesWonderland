using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Parts;

namespace MariesWonderland.Services;

public class PartsService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.Parts.PartsService.PartsServiceBase
{
    private const int PartsMaxLevel = 15;

    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>
    /// Sells one or more parts from the player's inventory, awarding gold based on each part's rarity and level.
    /// Protected parts are silently skipped.
    /// </summary>
    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int totalGold = 0;

        foreach (string uuid in request.UserPartsUuid)
        {
            EntityIUserParts? part = null;
            foreach (EntityIUserParts p in userDb.EntityIUserParts)
            {
                if (p.UserPartsUuid == uuid)
                {
                    part = p;
                    break;
                }
            }

            // Skip unknown or protected parts
            if (part == null || part.IsProtected)
            {
                continue;
            }

            EntityMParts? partDef = null;
            foreach (EntityMParts pd in _masterDb.EntityMParts)
            {
                if (pd.PartsId == part.PartsId)
                {
                    partDef = pd;
                    break;
                }
            }

            if (partDef == null)
            {
                continue;
            }

            // Look up sell price based on rarity
            EntityMPartsRarity? rarityRow = null;
            foreach (EntityMPartsRarity r in _masterDb.EntityMPartsRarity)
            {
                if (r.RarityType == partDef.RarityType)
                {
                    rarityRow = r;
                    break;
                }
            }

            if (rarityRow != null)
            {
                int gold = EvaluateNumericalFunction(rarityRow.SellPriceNumericalFunctionId, part.Level);
                totalGold += gold;
            }

            userDb.EntityIUserParts.Remove(part);
        }

        // Award total gold earned from the sale
        if (totalGold > 0)
        {
            AddGold(userDb, userId, totalGold);
        }

        return Task.FromResult(new SellResponse());
    }

    /// <summary>
    /// Marks the specified parts as protected, preventing them from being accidentally sold.
    /// </summary>
    public override Task<ProtectResponse> Protect(ProtectRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (string uuid in request.UserPartsUuid)
        {
            foreach (EntityIUserParts p in userDb.EntityIUserParts)
            {
                if (p.UserPartsUuid == uuid)
                {
                    p.IsProtected = true;
                    break;
                }
            }
        }

        return Task.FromResult(new ProtectResponse());
    }

    /// <summary>
    /// Removes the protection flag from the specified parts, allowing them to be sold.
    /// </summary>
    public override Task<UnprotectResponse> Unprotect(UnprotectRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (string uuid in request.UserPartsUuid)
        {
            foreach (EntityIUserParts p in userDb.EntityIUserParts)
            {
                if (p.UserPartsUuid == uuid)
                {
                    p.IsProtected = false;
                    break;
                }
            }
        }

        return Task.FromResult(new UnprotectResponse());
    }

    /// <summary>
    /// Attempts to enhance a part by one level, deducting gold and performing a probability-based success roll.
    /// Returns whether the level-up succeeded. Enhancement fails immediately if the part is at max level or gold is insufficient.
    /// </summary>
    public override Task<EnhanceResponse> Enhance(EnhanceRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserParts? part = null;
        foreach (EntityIUserParts p in userDb.EntityIUserParts)
        {
            if (p.UserPartsUuid == request.UserPartsUuid)
            {
                part = p;
                break;
            }
        }

        // Reject if part not found or already at max level
        if (part == null || part.Level >= PartsMaxLevel)
        {
            return Task.FromResult(new EnhanceResponse { IsSuccess = false });
        }

        EntityMParts? partDef = null;
        foreach (EntityMParts pd in _masterDb.EntityMParts)
        {
            if (pd.PartsId == part.PartsId)
            {
                partDef = pd;
                break;
            }
        }

        if (partDef == null)
        {
            return Task.FromResult(new EnhanceResponse { IsSuccess = false });
        }

        EntityMPartsRarity? rarityRow = null;
        foreach (EntityMPartsRarity r in _masterDb.EntityMPartsRarity)
        {
            if (r.RarityType == partDef.RarityType)
            {
                rarityRow = r;
                break;
            }
        }

        if (rarityRow == null)
        {
            return Task.FromResult(new EnhanceResponse { IsSuccess = false });
        }

        // Look up gold cost from price group
        int goldCost = LookupPartsLevelUpPrice(rarityRow.PartsLevelUpPriceGroupId, part.Level);

        EntityIUserConsumableItem? gold = FindConsumableItem(userDb, _gameConfig.ConsumableItemIdForGold);
        if (gold == null || gold.Count < goldCost)
        {
            return Task.FromResult(new EnhanceResponse { IsSuccess = false });
        }

        // Deduct gold before rolling — cost applies even on failure
        gold.Count -= goldCost;

        // Look up success rate
        int successRate = LookupPartsLevelUpRate(rarityRow.PartsLevelUpRateGroupId, part.Level);

        // Roll for enhancement success (permil)
        bool isSuccess = Random.Shared.Next(1000) < successRate;
        if (isSuccess)
        {
            part.Level++;
        }

        return Task.FromResult(new EnhanceResponse { IsSuccess = isSuccess });
    }

    /// <summary>
    /// Sets the display name of a parts preset if it exists.
    /// </summary>
    public override Task<UpdatePresetNameResponse> UpdatePresetName(UpdatePresetNameRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserPartsPreset? preset = null;
        foreach (EntityIUserPartsPreset p in userDb.EntityIUserPartsPreset)
        {
            if (p.UserPartsPresetNumber == request.UserPartsPresetNumber)
            {
                preset = p;
                break;
            }
        }

        if (preset != null)
        {
            preset.Name = request.Name;
        }

        return Task.FromResult(new UpdatePresetNameResponse());
    }

    /// <summary>
    /// Assigns a tag number to a parts preset if it exists.
    /// </summary>
    public override Task<UpdatePresetTagNumberResponse> UpdatePresetTagNumber(UpdatePresetTagNumberRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserPartsPreset? preset = null;
        foreach (EntityIUserPartsPreset p in userDb.EntityIUserPartsPreset)
        {
            if (p.UserPartsPresetNumber == request.UserPartsPresetNumber)
            {
                preset = p;
                break;
            }
        }

        if (preset != null)
        {
            preset.UserPartsPresetTagNumber = request.UserPartsPresetTagNumber;
        }

        return Task.FromResult(new UpdatePresetTagNumberResponse());
    }

    /// <summary>
    /// Sets the display name of a preset tag if it exists.
    /// </summary>
    public override Task<UpdatePresetTagNameResponse> UpdatePresetTagName(UpdatePresetTagNameRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserPartsPresetTag? tag = null;
        foreach (EntityIUserPartsPresetTag t in userDb.EntityIUserPartsPresetTag)
        {
            if (t.UserPartsPresetTagNumber == request.UserPartsPresetTagNumber)
            {
                tag = t;
                break;
            }
        }

        if (tag != null)
        {
            tag.Name = request.Name;
        }

        return Task.FromResult(new UpdatePresetTagNameResponse());
    }

    /// <summary>
    /// Overwrites the three parts slots of a preset with the specified part UUIDs, creating the preset if needed.
    /// </summary>
    public override Task<ReplacePresetResponse> ReplacePreset(ReplacePresetRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserPartsPreset? preset = null;
        foreach (EntityIUserPartsPreset p in userDb.EntityIUserPartsPreset)
        {
            if (p.UserPartsPresetNumber == request.UserPartsPresetNumber)
            {
                preset = p;
                break;
            }
        }

        if (preset != null)
        {
            preset.UserPartsUuid01 = request.UserPartsUuid01;
            preset.UserPartsUuid02 = request.UserPartsUuid02;
            preset.UserPartsUuid03 = request.UserPartsUuid03;
        }
        else
        {
            userDb.EntityIUserPartsPreset.Add(new EntityIUserPartsPreset
            {
                UserId = userId,
                UserPartsPresetNumber = request.UserPartsPresetNumber,
                UserPartsUuid01 = request.UserPartsUuid01,
                UserPartsUuid02 = request.UserPartsUuid02,
                UserPartsUuid03 = request.UserPartsUuid03,
                Name = string.Empty
            });
        }

        return Task.FromResult(new ReplacePresetResponse());
    }

    /// <summary>
    /// Copies the three parts slots from one preset to another, creating the destination preset if it does not yet exist.
    /// The source preset must exist; if it doesn't, no changes are made.
    /// </summary>
    public override Task<CopyPresetResponse> CopyPreset(CopyPresetRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        // Find both source and destination presets in a single pass
        EntityIUserPartsPreset? from = null;
        EntityIUserPartsPreset? to = null;
        foreach (EntityIUserPartsPreset p in userDb.EntityIUserPartsPreset)
        {
            if (p.UserPartsPresetNumber == request.FromUserPartsPresetNumber)
            {
                from = p;
            }
            if (p.UserPartsPresetNumber == request.ToUserPartsPresetNumber)
            {
                to = p;
            }
        }

        if (from != null)
        {
            if (to != null)
            {
                to.UserPartsUuid01 = from.UserPartsUuid01;
                to.UserPartsUuid02 = from.UserPartsUuid02;
                to.UserPartsUuid03 = from.UserPartsUuid03;
            }
            else
            {
                userDb.EntityIUserPartsPreset.Add(new EntityIUserPartsPreset
                {
                    UserId = userId,
                    UserPartsPresetNumber = request.ToUserPartsPresetNumber,
                    UserPartsUuid01 = from.UserPartsUuid01,
                    UserPartsUuid02 = from.UserPartsUuid02,
                    UserPartsUuid03 = from.UserPartsUuid03,
                    Name = string.Empty
                });
            }
        }

        return Task.FromResult(new CopyPresetResponse());
    }

    /// <summary>
    /// Clears all three parts slots in a preset, emptying the loadout without deleting the preset record.
    /// </summary>
    public override Task<RemovePresetResponse> RemovePreset(RemovePresetRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserPartsPreset? preset = null;
        foreach (EntityIUserPartsPreset p in userDb.EntityIUserPartsPreset)
        {
            if (p.UserPartsPresetNumber == request.UserPartsPresetNumber)
            {
                preset = p;
                break;
            }
        }

        if (preset != null)
        {
            preset.UserPartsUuid01 = string.Empty;
            preset.UserPartsUuid02 = string.Empty;
            preset.UserPartsUuid03 = string.Empty;
        }

        return Task.FromResult(new RemovePresetResponse());
    }

    /// <summary>
    /// Returns the gold cost to enhance a part at the given level, using the rarity's price group.
    /// Selects the row with the highest <c>LevelLowerLimit</c> that does not exceed the current level.
    /// </summary>
    private int LookupPartsLevelUpPrice(int priceGroupId, int level)
    {
        int price = 0;
        foreach (EntityMPartsLevelUpPriceGroup row in _masterDb.EntityMPartsLevelUpPriceGroup)
        {
            if (row.PartsLevelUpPriceGroupId == priceGroupId && row.LevelLowerLimit <= level)
            {
                if (row.LevelLowerLimit >= (price > 0 ? price : 0))
                {
                    price = row.Gold;
                }
            }
        }
        return price;
    }

    /// <summary>
    /// Returns the enhancement success rate (permil) for a part at the given level, using the rarity's rate group.
    /// Defaults to 1000 (100%) when no matching row is found.
    /// </summary>
    private int LookupPartsLevelUpRate(int rateGroupId, int level)
    {
        int rate = 1000; // Default 100% success
        int bestLowerLimit = -1;
        foreach (EntityMPartsLevelUpRateGroup row in _masterDb.EntityMPartsLevelUpRateGroup)
        {
            if (row.PartsLevelUpRateGroupId == rateGroupId && row.LevelLowerLimit <= level && row.LevelLowerLimit > bestLowerLimit)
            {
                bestLowerLimit = row.LevelLowerLimit;
                rate = row.SuccessRatePermil;
            }
        }
        return rate;
    }

    /// <summary>
    /// Credits gold to the user's consumable inventory, creating the inventory entry if one does not yet exist.
    /// </summary>
    private void AddGold(DarkUserMemoryDatabase userDb, long userId, int amount)
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
            gold.Count += amount;
        }
        else
        {
            userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
            {
                UserId = userId,
                ConsumableItemId = _gameConfig.ConsumableItemIdForGold,
                Count = amount,
                FirstAcquisitionDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
    }

    /// <summary>
    /// Returns the user's consumable item record for the given item ID, or <see langword="null"/> if not found.
    /// </summary>
    private static EntityIUserConsumableItem? FindConsumableItem(DarkUserMemoryDatabase userDb, int itemId)
    {
        foreach (EntityIUserConsumableItem ci in userDb.EntityIUserConsumableItem)
        {
            if (ci.ConsumableItemId == itemId)
            {
                return ci;
            }
        }
        return null;
    }

    /// <summary>
    /// Evaluates a master data numerical function against an input value (e.g., computes sell price for a given part level).
    /// Returns 0 if the function definition is not found or the formula type is unrecognised.
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

        // Collect and sort parameters by index
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

        // Dispatch to the appropriate formula type
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
    /// Computes <c>p[0] * (value - 1)^p[1]</c> using iterative multiplication.
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
}
