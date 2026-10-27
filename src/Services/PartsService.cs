using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Parts;

namespace MariesWonderland.Services;

public class PartsService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig, EnhanceCampaignHelper enhanceCampaigns, MissionTracker missions, TimeProvider timeProvider)
    : MariesWonderland.Proto.Parts.PartsService.PartsServiceBase
{
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    private const int PartsMaxLevel = 15;

    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly EnhanceCampaignHelper _enhanceCampaigns = enhanceCampaigns;

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
                int gold = NumericalFunctionHelper.Evaluate(_masterDb, rarityRow.SellPriceNumericalFunctionId, part.Level);
                totalGold += gold;
            }

            userDb.EntityIUserParts.Remove(part);
            userDb.EntityIUserPartsStatusSub.RemoveAll(s => s.UserPartsUuid == uuid);
        }

        // Award total gold earned from the sale
        if (totalGold > 0)
        {
            AddGold(userDb, totalGold);
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

        // Look up success rate, raised by enhance campaigns
        int successRate = _enhanceCampaigns.ApplyPartsSuccessRate(
            LookupPartsLevelUpRate(rarityRow.PartsLevelUpRateGroupId, part.Level),
            partDef,
            userDb,
            _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());

        // Roll for enhancement success (permil)
        bool isSuccess = Random.Shared.Next(1000) < successRate;
        if (isSuccess)
        {
            part.Level++;
            GrantUnlockedSubStatuses(userDb, part);
        }

        _missions.Record(userDb, MissionClearConditionType.PARTS_ENHANCE_BY_COUNT, 1, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());

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
    /// Sets the display name of a preset tag, creating the tag when it has no row yet.
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
        else
        {
            // A tag only has a row once it is named (a new account has none), so naming one creates it.
            userDb.EntityIUserPartsPresetTag.Add(new EntityIUserPartsPresetTag
            {
                UserId = userId,
                UserPartsPresetTagNumber = request.UserPartsPresetTagNumber,
                Name = request.Name
            });
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
        int bestLowerLimit = -1;
        foreach (EntityMPartsLevelUpPriceGroup row in _masterDb.EntityMPartsLevelUpPriceGroup)
        {
            if (row.PartsLevelUpPriceGroupId == priceGroupId && row.LevelLowerLimit <= level && row.LevelLowerLimit > bestLowerLimit)
            {
                bestLowerLimit = row.LevelLowerLimit;
                price = row.Gold;
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
    /// Creates any newly unlocked memoir sub-status records for the part's current enhancement level.
    /// Existing slots are preserved.
    /// </summary>
    private void GrantUnlockedSubStatuses(DarkUserMemoryDatabase userDb, EntityIUserParts part)
    {
        EntityMPartsEnhanced? enhancedRow = null;
        foreach (EntityMPartsEnhanced row in _masterDb.EntityMPartsEnhanced)
        {
            if (row.PartsId == part.PartsId && row.Level == part.Level)
            {
                enhancedRow = row;
                break;
            }
        }

        if (enhancedRow == null || enhancedRow.SubStatusCount <= 0)
        {
            return;
        }

        foreach (EntityMPartsEnhancedSubStatus row in _masterDb.EntityMPartsEnhancedSubStatus)
        {
            if (row.PartsEnhancedId != enhancedRow.PartsEnhancedId || row.StatusIndex > enhancedRow.SubStatusCount)
            {
                continue;
            }

            bool alreadyUnlocked = false;
            foreach (EntityIUserPartsStatusSub existing in userDb.EntityIUserPartsStatusSub)
            {
                if (existing.UserPartsUuid == part.UserPartsUuid && existing.StatusIndex == row.StatusIndex)
                {
                    alreadyUnlocked = true;
                    break;
                }
            }

            if (alreadyUnlocked)
            {
                continue;
            }

            userDb.EntityIUserPartsStatusSub.Add(new EntityIUserPartsStatusSub
            {
                UserId = part.UserId,
                UserPartsUuid = part.UserPartsUuid,
                StatusIndex = row.StatusIndex,
                PartsStatusSubLotteryId = row.PartsStatusSubLotteryId,
                Level = row.Level,
                StatusKindType = row.StatusKindType,
                StatusCalculationType = row.StatusCalculationType,
                StatusChangeValue = row.FixedStatusChangeValue
            });
        }
    }

    /// <summary>
    /// Credits gold to the user's consumable inventory, creating the inventory entry if one does not yet exist.
    /// </summary>
    private void AddGold(DarkUserMemoryDatabase userDb, int amount)
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
                UserId = userDb.UserId,
                ConsumableItemId = _gameConfig.ConsumableItemIdForGold,
                Count = amount,
                FirstAcquisitionDatetime = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds()
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
}
