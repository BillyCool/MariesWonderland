using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gacha;

namespace MariesWonderland.Services;

public class GachaService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.Gacha.GachaService.GachaServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    private const int PhaseIdMultiplier = 10;
    private const int StepUpGroupDivisor = 1000;
    private const int ChapterGachaIdBase = 200000;
    private const int PremiumSinglePullPrice = 300;
    private const int PremiumMultiPullPrice = 3000;
    private const int PremiumMultiPullCount = 10;
    private const int StepUpStep1Cost = 2000;
    private const int StepUpStep3Cost = 3000;
    private const int StepUpStep5Cost = 5000;
    private const int StepUpFreeCost = 0;
    private const int ConsumableIdPremiumTicket = 1;
    private const int ConsumableIdChapterTicket = 2;
    private const int MedalCountCap = 9999;
    private const int DupGradeMin = 2;
    private const int DupGradeRange = 4;

    /// <summary>
    /// Returns the filtered and sorted gacha banner catalog, including user gacha state.
    /// </summary>
    public override Task<GetGachaListResponse> GetGachaList(GetGachaListRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        List<GachaEntry> catalog = BuildCatalog();
        catalog.Sort((a, b) => a.SortOrder != b.SortOrder
            ? a.SortOrder.CompareTo(b.SortOrder)
            : a.GachaId.CompareTo(b.GachaId));

        // Build medal conversion data from master data
        ConvertedGachaMedal convertedMedal = new()
        {
            ObtainPossession = new ConsumableItemPossession()
        };

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        foreach (GachaEntry entry in catalog)
        {
            if (entry.Medal == null || entry.EndDatetime == 0)
                continue;
            if (nowMs < entry.EndDatetime)
                continue;

            EntitySGachaBannerState? bs = FindBannerState(userDb, entry.GachaId);
            if (bs == null || bs.MedalCount <= 0)
                continue;

            int conversionRate = entry.Medal.ConversionRate > 0 ? entry.Medal.ConversionRate : 1;
            int bookmarkCount = bs.MedalCount * conversionRate;

            PossessionHelper.Apply(userDb, PossessionType.CONSUMABLE_ITEM, entry.Medal.ConsumableItemId, bookmarkCount, _masterDb);

            convertedMedal.ConvertedMedalPossession.Add(new ConsumableItemPossession
            {
                ConsumableItemId = entry.Medal.ConsumableItemId,
                Count = bookmarkCount
            });

            bs.MedalCount = 0;
        }

        GetGachaListResponse response = new()
        {
            ConvertedGachaMedal = convertedMedal
        };

        foreach (GachaEntry entry in catalog)
        {
            if (request.GachaLabelType.Count > 0 && !request.GachaLabelType.Contains((int)entry.LabelType))
                continue;

            // Skip portal cage and recycle label types
            if (entry.LabelType is GachaLabelType.PORTAL_CAGE or GachaLabelType.RECYCLE)
                continue;

            EntitySGachaBannerState? bs = FindBannerState(userDb, entry.GachaId);
            response.Gacha.Add(ToProtoGacha(entry, bs));
        }

        return Task.FromResult(response);
    }

    /// <summary>
    /// Returns gacha details for the requested gacha IDs.
    /// </summary>
    public override Task<GetGachaResponse> GetGacha(GetGachaRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        List<GachaEntry> catalog = BuildCatalog();
        GetGachaResponse response = new();

        foreach (GachaEntry entry in catalog)
        {
            if (request.GachaId.Contains(entry.GachaId))
            {
                EntitySGachaBannerState? bs = FindBannerState(userDb, entry.GachaId);
                response.Gacha[entry.GachaId] = ToProtoGacha(entry, bs);
            }
        }

        return Task.FromResult(response);
    }

    /// <summary>
    /// Executes a gacha draw: deducts currency, rolls items from banner-specific pools, grants possessions, and returns results.
    /// </summary>
    public override Task<DrawResponse> Draw(DrawRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        List<GachaEntry> catalog = BuildCatalog();
        GachaEntry? entry = null;
        foreach (GachaEntry e in catalog)
        {
            if (e.GachaId == request.GachaId)
            {
                entry = e;
                break;
            }
        }

        if (entry == null)
        {
            return Task.FromResult(new DrawResponse());
        }

        int execCount = request.ExecCount > 0 ? request.ExecCount : 1;

        // Find the price phase from the catalog entry's phases
        PricePhaseEntry phase = FindPhase(entry, request.GachaPricePhaseId);

        int totalCost = phase.Price * execCount;
        int drawCount = phase.DrawCount * execCount;

        // Deduct currency based on price type
        if (totalCost > 0)
        {
            DeductPrice(userDb, phase.PriceType, phase.PriceId, totalCost);
        }

        // Find or create banner state
        EntitySGachaBannerState bannerState = GetOrCreateBannerState(userDb, entry.GachaId);

        // Draw items based on label type
        List<DrawnItem> drawnItems;
        bool isMaterialDraw = entry.LabelType is GachaLabelType.CHAPTER or GachaLabelType.RECYCLE or GachaLabelType.PORTAL_CAGE;

        if (isMaterialDraw)
        {
            drawnItems = DrawMaterials(drawCount);
        }
        else
        {
            drawnItems = DrawPremium(entry, phase, drawCount);
        }

        // Step-up progression
        if (entry.ModeType == GachaModeType.STEPUP)
        {
            bannerState.StepNumber++;
            if (bannerState.StepNumber > entry.MaxStepNumber)
            {
                bannerState.StepNumber = 1;
                bannerState.LoopCount++;
            }
        }

        // Medal tracking
        int medalBonus = 0;
        if (entry.Medal != null && entry.Medal.GachaMedalId != 0)
        {
            medalBonus = drawCount;
            bannerState.MedalCount += medalBonus;
            if (bannerState.MedalCount > MedalCountCap)
                bannerState.MedalCount = MedalCountCap;
        }

        bannerState.DrawCount += drawCount;

        // Build set of owned costumes/weapons for duplicate detection
        HashSet<int> ownedCostumeIds = [];
        foreach (EntityIUserCostume c in userDb.EntityIUserCostume)
            ownedCostumeIds.Add(c.CostumeId);

        HashSet<int> ownedWeaponIds = [];
        foreach (EntityIUserWeapon w in userDb.EntityIUserWeapon)
            ownedWeaponIds.Add(w.WeaponId);

        // Build costume->weapon pairing map
        Dictionary<int, int> costumeWeaponMap = BuildCostumeWeaponMap();

        // Grant drawn items and build response
        List<DrawGachaOddsItem> gachaResults = new(drawCount);
        for (int i = 0; i < drawnItems.Count; i++)
        {
            DrawnItem item = drawnItems[i];

            if (isMaterialDraw)
            {
                // Material draws: grant material and build simple result
                PossessionHelper.Apply(userDb, (PossessionType)item.PossessionType, item.PossessionId, 1, _masterDb);
                bool isNew = !IsOwnedByType(item.PossessionType, item.PossessionId, ownedCostumeIds, ownedWeaponIds, userDb);

                gachaResults.Add(new DrawGachaOddsItem
                {
                    GachaItem = new GachaItem
                    {
                        PossessionType = item.PossessionType,
                        PossessionId = item.PossessionId,
                        Count = 1,
                        IsNew = isNew
                    },
                    GachaItemBonus = new GachaItem()
                });
            }
            else if (item.PossessionType == (int)PossessionType.COSTUME)
            {
                bool isNew = !ownedCostumeIds.Contains(item.PossessionId);

                if (!isNew)
                {
                    // Costume duplicate
                    int dupGrade = Random.Shared.Next(DupGradeRange) + DupGradeMin;
                    gachaResults.Add(new DrawGachaOddsItem
                    {
                        GachaItem = new GachaItem
                        {
                            PossessionType = item.PossessionType,
                            PossessionId = item.PossessionId,
                            Count = 1,
                            IsNew = false
                        },
                        GachaItemBonus = new GachaItem(),
                        DuplicationBonusGrade = dupGrade
                    });
                }
                else
                {
                    PossessionHelper.GrantCostume(userDb, item.PossessionId, _masterDb);
                    ownedCostumeIds.Add(item.PossessionId);

                    // Bonus weapon via costume->weapon pairing map
                    GachaItem bonusGachaItem = new();
                    if (costumeWeaponMap.TryGetValue(item.PossessionId, out int pairedWeaponId) && pairedWeaponId > 0)
                    {
                        WeaponHelper.GrantWeapon(userDb, pairedWeaponId, _masterDb);
                        bonusGachaItem = new GachaItem
                        {
                            PossessionType = (int)PossessionType.WEAPON,
                            PossessionId = pairedWeaponId,
                            Count = 1,
                            IsNew = !ownedWeaponIds.Contains(pairedWeaponId)
                        };
                        ownedWeaponIds.Add(pairedWeaponId);
                    }

                    gachaResults.Add(new DrawGachaOddsItem
                    {
                        GachaItem = new GachaItem
                        {
                            PossessionType = item.PossessionType,
                            PossessionId = item.PossessionId,
                            Count = 1,
                            IsNew = true
                        },
                        GachaItemBonus = bonusGachaItem
                    });
                }
            }
            else
            {
                // Weapon
                bool isNew = !ownedWeaponIds.Contains(item.PossessionId);
                WeaponHelper.GrantWeapon(userDb, item.PossessionId, _masterDb);
                ownedWeaponIds.Add(item.PossessionId);

                gachaResults.Add(new DrawGachaOddsItem
                {
                    GachaItem = new GachaItem
                    {
                        PossessionType = item.PossessionType,
                        PossessionId = item.PossessionId,
                        Count = 1,
                        IsNew = isNew
                    },
                    GachaItemBonus = new GachaItem()
                });
            }

            // Medal bonus per draw
            if (medalBonus > 0 && entry.Medal != null)
            {
                gachaResults[^1].MedalBonus = new GachaBonus
                {
                    PossessionType = (int)PossessionType.CONSUMABLE_ITEM,
                    PossessionId = entry.Medal.ConsumableItemId,
                    Count = 0
                };
            }
        }

        // Grant medal consumable items (1 per draw)
        if (entry.Medal != null && medalBonus > 0)
        {
            PossessionHelper.Apply(userDb, PossessionType.CONSUMABLE_ITEM, entry.Medal.ConsumableItemId, medalBonus, _masterDb);
        }

        DrawResponse response = new()
        {
            NextGacha = ToProtoGacha(entry, bannerState)
        };
        response.GachaResult.AddRange(gachaResults);

        return Task.FromResult(response);
    }

    /// <summary>
    /// Resets a box gacha banner: clears drew counts and increments the box number.
    /// </summary>
    public override Task<ResetBoxGachaResponse> ResetBoxGacha(ResetBoxGachaRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        List<GachaEntry> catalog = BuildCatalog();
        GachaEntry? entry = null;
        foreach (GachaEntry e in catalog)
        {
            if (e.GachaId == request.GachaId)
            {
                entry = e;
                break;
            }
        }

        if (entry == null)
        {
            return Task.FromResult(new ResetBoxGachaResponse());
        }

        EntitySGachaBannerState bannerState = GetOrCreateBannerState(userDb, request.GachaId);
        bannerState.BoxDrewCounts = [];
        bannerState.BoxNumber++;

        return Task.FromResult(new ResetBoxGachaResponse
        {
            Gacha = ToProtoGacha(entry, bannerState)
        });
    }

    /// <summary>
    /// Returns the current daily reward gacha availability and draw count.
    /// </summary>
    public override Task<GetRewardGachaResponse> GetRewardGacha(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int maxCount = _gameConfig.RewardGachaDailyMaxCount > 0 ? _gameConfig.RewardGachaDailyMaxCount : 5;
        long todayStart = StartOfDayMillis();

        EntitySGachaRewardState? rewardState = null;
        foreach (EntitySGachaRewardState rs in userDb.EntitySGachaRewardState)
        {
            if (rs.UserId == userId)
            {
                rewardState = rs;
                break;
            }
        }

        int drawCount = rewardState?.TodaysCurrentDrawCount ?? 0;
        if (rewardState == null || rewardState.LastRewardDrawDate < todayStart)
        {
            drawCount = 0;
        }

        return Task.FromResult(new GetRewardGachaResponse
        {
            Available = drawCount < maxCount,
            TodaysCurrentDrawCount = drawCount,
            DailyMaxCount = maxCount
        });
    }

    /// <summary>
    /// Executes a free daily reward draw: grants a random material and updates daily draw state.
    /// </summary>
    public override Task<RewardDrawResponse> RewardDraw(RewardDrawRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long todayStart = StartOfDayMillis();

        int maxCount = _gameConfig.RewardGachaDailyMaxCount > 0 ? _gameConfig.RewardGachaDailyMaxCount : 5;

        EntitySGachaRewardState? rewardState = null;
        foreach (EntitySGachaRewardState rs in userDb.EntitySGachaRewardState)
        {
            if (rs.UserId == userId)
            {
                rewardState = rs;
                break;
            }
        }

        if (rewardState == null)
        {
            rewardState = new EntitySGachaRewardState
            {
                UserId = userId
            };
            userDb.EntitySGachaRewardState.Add(rewardState);
        }

        int currentCount = rewardState.TodaysCurrentDrawCount;
        if (rewardState.LastRewardDrawDate < todayStart)
        {
            currentCount = 0;
        }

        int remaining = maxCount - currentCount;
        if (remaining <= 0)
        {
            return Task.FromResult(new RewardDrawResponse());
        }

        int drawCount = Math.Min(1, remaining);

        // Draw random materials
        List<EntityMMaterial> materials = [.. _masterDb.EntityMMaterial];
        List<(int PossessionType, int PossessionId)> drawnItems = [];

        for (int i = 0; i < drawCount && materials.Count > 0; i++)
        {
            EntityMMaterial mat = materials[Random.Shared.Next(materials.Count)];
            drawnItems.Add(((int)PossessionType.MATERIAL, mat.MaterialId));
            PossessionHelper.Apply(userDb, PossessionType.MATERIAL, mat.MaterialId, 1, _masterDb);
        }

        int newCount = currentCount + drawCount;
        rewardState.TodaysCurrentDrawCount = newCount;
        rewardState.DailyMaxCount = maxCount;
        rewardState.LastRewardDrawDate = nowMs;
        rewardState.RewardAvailable = newCount < maxCount;

        // Build ownership maps
        HashSet<int> ownedCostumeIds = [];
        foreach (EntityIUserCostume c in userDb.EntityIUserCostume)
            ownedCostumeIds.Add(c.CostumeId);

        HashSet<int> ownedWeaponIds = [];
        foreach (EntityIUserWeapon w in userDb.EntityIUserWeapon)
            ownedWeaponIds.Add(w.WeaponId);

        RewardDrawResponse response = new();
        foreach ((int possType, int possId) in drawnItems)
        {
            bool isOwned = IsOwnedByType(possType, possId, ownedCostumeIds, ownedWeaponIds, userDb);
            response.RewardGachaResult.Add(new RewardGachaItem
            {
                PossessionType = possType,
                PossessionId = possId,
                Count = 1,
                IsNew = !isOwned
            });
        }

        return Task.FromResult(response);
    }

    #region Catalog Building

    /// <summary>
    /// Builds the gacha catalog from MomBanner + GachaMedal master data, grouping step-up banners
    /// and including chapter banners (common_ prefix) that don't require medal entries.
    /// </summary>
    private List<GachaEntry> BuildCatalog()
    {
        long startDt = DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeMilliseconds();
        long endDt = DateTimeOffset.UtcNow.AddDays(3).ToUnixTimeMilliseconds();

        Dictionary<int, EntityMGachaMedal> medalByGachaId = [];
        foreach (EntityMGachaMedal medal in _masterDb.EntityMGachaMedal)
        {
            medalByGachaId[medal.ShopTransitionGachaId] = medal;
        }

        Dictionary<int, (GachaEntry Entry, int StepCount)> stepUpGroups = [];
        List<GachaEntry> entries = [];
        foreach (EntityMMomBanner banner in _masterDb.EntityMMomBanner)
        {
            if (banner.DestinationDomainType != DomainType.GACHA)
                continue;

            int gachaId = banner.DestinationDomainId;

            // Step-up banners: group by truncated ID
            if (banner.BannerAssetName.StartsWith("step_up_", StringComparison.Ordinal))
            {
                if (!medalByGachaId.TryGetValue(gachaId, out EntityMGachaMedal? stepMedal))
                    continue;

                int groupId = gachaId / StepUpGroupDivisor;
                if (!stepUpGroups.ContainsKey(groupId))
                {
                    stepUpGroups[groupId] = (new GachaEntry
                    {
                        GachaId = groupId,
                        LabelType = GachaLabelType.PREMIUM,
                        ModeType = GachaModeType.STEPUP,
                        StartDatetime = startDt,
                        EndDatetime = endDt,
                        Medal = stepMedal,
                        BannerAssetName = banner.BannerAssetName,
                        SortOrder = banner.SortOrderDesc,
                        DecorationType = GachaDecorationType.FESTIVAL
                    }, 1);
                }
                else
                {
                    var existing = stepUpGroups[groupId];
                    stepUpGroups[groupId] = (existing.Entry, existing.StepCount + 1);
                }
                continue;
            }

            bool isChapter = banner.BannerAssetName.StartsWith("common_", StringComparison.Ordinal);
            bool isLimited = banner.BannerAssetName.StartsWith("limited_", StringComparison.Ordinal);

            medalByGachaId.TryGetValue(gachaId, out EntityMGachaMedal? medal);

            // Chapter banners don't require medals; non-chapter banners do
            if (medal == null && !isChapter)
                continue;

            GachaLabelType labelType = GachaLabelType.PREMIUM;
            GachaModeType modeType = GachaModeType.BASIC;
            GachaDecorationType decoration = GachaDecorationType.NORMAL;

            if (isLimited)
                decoration = GachaDecorationType.FESTIVAL;

            if (isChapter)
            {
                labelType = GachaLabelType.CHAPTER;
                modeType = GachaModeType.BOX;
            }

            int relMainQuest = isChapter ? gachaId - ChapterGachaIdBase : 0;
            int descriptionTextId = isChapter ? gachaId : 0;

            entries.Add(new GachaEntry
            {
                GachaId = gachaId,
                LabelType = labelType,
                ModeType = modeType,
                StartDatetime = startDt,
                EndDatetime = endDt,
                Medal = medal,
                BannerAssetName = banner.BannerAssetName,
                SortOrder = banner.SortOrderDesc,
                DecorationType = decoration,
                RelatedMainQuestChapterId = relMainQuest,
                DescriptionTextId = descriptionTextId,
                PricePhases = isChapter ? BuildChapterPricePhases(gachaId) : BuildPremiumBasicPricePhases(gachaId)
            });
        }

        // Build step-up entries with proper price phases
        foreach (var (groupId, (entry, stepCount)) in stepUpGroups)
        {
            List<PricePhaseEntry> phases = BuildStepUpPricePhases(groupId, stepCount);
            int maxStep = 0;
            foreach (PricePhaseEntry p in phases)
            {
                if (p.StepNumber > maxStep)
                    maxStep = p.StepNumber;
            }

            entries.Add(entry with { PricePhases = phases, MaxStepNumber = maxStep });
        }

        return entries;
    }

    private static List<PricePhaseEntry> BuildPremiumBasicPricePhases(int gachaId)
    {
        return
        [
            new PricePhaseEntry
            {
                PhaseId = gachaId * PhaseIdMultiplier + 1,
                PriceType = (int)PriceType.GEM,
                Price = PremiumSinglePullPrice,
                RegularPrice = PremiumSinglePullPrice,
                DrawCount = 1
            },
            new PricePhaseEntry
            {
                PhaseId = gachaId * PhaseIdMultiplier + 2,
                PriceType = (int)PriceType.GEM,
                Price = PremiumMultiPullPrice,
                RegularPrice = PremiumMultiPullPrice,
                DrawCount = PremiumMultiPullCount,
                FixedRarityMin = (int)RarityType.S_RARE,
                FixedCount = 1
            },
            new PricePhaseEntry
            {
                PhaseId = gachaId * PhaseIdMultiplier + 3,
                PriceType = (int)PriceType.CONSUMABLE_ITEM,
                PriceId = ConsumableIdPremiumTicket,
                Price = 1,
                RegularPrice = 1,
                DrawCount = 1
            }
        ];
    }

    private static List<PricePhaseEntry> BuildStepUpPricePhases(int gachaId, int totalSteps)
    {
        int[] stepCosts = [StepUpStep1Cost, StepUpFreeCost, StepUpStep3Cost, StepUpFreeCost, StepUpStep5Cost];
        int count = Math.Min(totalSteps, stepCosts.Length);

        List<PricePhaseEntry> phases = new(count);
        for (int i = 0; i < count; i++)
        {
            int step = i + 1;
            int cost = stepCosts[i];
            int priceType = cost == 0 ? (int)PriceType.GEM : (int)PriceType.PAID_GEM;

            int fixedRarityMin = 0;
            int fixedCount = 0;
            if (step == count)
            {
                fixedRarityMin = (int)RarityType.SS_RARE;
                fixedCount = 1;
            }

            phases.Add(new PricePhaseEntry
            {
                PhaseId = gachaId * PhaseIdMultiplier + step,
                PriceType = priceType,
                Price = cost,
                RegularPrice = PremiumMultiPullPrice,
                DrawCount = PremiumMultiPullCount,
                FixedRarityMin = fixedRarityMin,
                FixedCount = fixedCount,
                LimitExecCount = 1,
                StepNumber = step
            });
        }
        return phases;
    }

    private static List<PricePhaseEntry> BuildChapterPricePhases(int gachaId)
    {
        return
        [
            new PricePhaseEntry
            {
                PhaseId = gachaId * PhaseIdMultiplier + 1,
                PriceType = (int)PriceType.CONSUMABLE_ITEM,
                PriceId = ConsumableIdChapterTicket,
                Price = 1,
                RegularPrice = 1,
                DrawCount = 1
            },
            new PricePhaseEntry
            {
                PhaseId = gachaId * PhaseIdMultiplier + 2,
                PriceType = (int)PriceType.CONSUMABLE_ITEM,
                PriceId = ConsumableIdChapterTicket,
                Price = 10,
                RegularPrice = 10,
                DrawCount = PremiumMultiPullCount
            }
        ];
    }

    #endregion

    #region Proto Conversion

    /// <summary>
    /// Converts a GachaEntry to the protobuf Gacha message with proper price phases, mode,
    /// and user banner state.
    /// </summary>
    private static Gacha ToProtoGacha(GachaEntry entry, EntitySGachaBannerState? bannerState = null)
    {
        Gacha gacha = new()
        {
            GachaId = entry.GachaId,
            GachaLabelType = (int)entry.LabelType,
            GachaModeType = (int)entry.ModeType,
            GachaAutoResetType = (int)GachaAutoResetType.NONE,
            GachaAutoResetPeriod = 0,
            NextAutoResetDatetime = new Timestamp { Seconds = 0 },
            IsUserGachaUnlock = true,
            StartDatetime = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(entry.StartDatetime)),
            EndDatetime = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(entry.EndDatetime)),
            RelatedMainQuestChapterId = entry.RelatedMainQuestChapterId,
            RelatedEventQuestChapterId = 0,
            PromotionMovieAssetId = 0,
            GachaMedalId = entry.Medal?.GachaMedalId ?? 0,
            GachaDecorationType = (int)entry.DecorationType,
            SortOrder = entry.SortOrder,
            IsInactive = false,
            InformationId = 0
        };

        gacha.GachaUnlockCondition.Add(new GachaUnlockCondition
        {
            GachaUnlockConditionType = (int)GachaUnlockConditionType.NONE,
            ConditionValue = 0
        });

        // Build price phases from catalog entry
        foreach (PricePhaseEntry p in entry.PricePhases)
        {
            bool isEnabled = true;
            if (entry.ModeType == GachaModeType.STEPUP && bannerState != null)
            {
                int currentStep = bannerState.StepNumber > 0 ? bannerState.StepNumber : 1;
                isEnabled = p.StepNumber == currentStep;
            }

            int limitExec = p.LimitExecCount > 0 ? p.LimitExecCount : 999;

            gacha.GachaPricePhase.Add(new GachaPricePhase
            {
                GachaPricePhaseId = p.PhaseId,
                IsEnabled = isEnabled,
                EndDatetime = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(entry.EndDatetime)),
                PriceType = p.PriceType,
                PriceId = p.PriceId,
                Price = p.Price,
                RegularPrice = p.RegularPrice,
                DrawCount = p.DrawCount,
                LimitExecCount = limitExec,
                EachMaxExecCount = p.DrawCount,
                GachaOddsFixedRarity = new GachaOddsFixedRarity
                {
                    FixedRarityTypeLowerLimit = p.FixedRarityMin,
                    FixedCount = p.FixedCount
                },
                GachaBadgeType = 1
            });
        }

        int boxNumber = bannerState?.BoxNumber > 0 ? bannerState.BoxNumber : 1;
        int stepNumber = bannerState?.StepNumber > 0 ? bannerState.StepNumber : 1;
        int loopCount = bannerState?.LoopCount ?? 0;

        gacha.GachaMode = entry.ModeType switch
        {
            GachaModeType.BOX => new GachaModeBoxComposition
            {
                GachaBoxGroupId = entry.GachaId,
                BoxNumber = boxNumber,
                CurrentBoxNumber = boxNumber,
                GachaAssetName = entry.BannerAssetName,
                GachaPricePhaseId = entry.PricePhases.Count > 0 ? entry.PricePhases[0].PhaseId : 0
            }.ToByteString(),
            GachaModeType.STEPUP => new GachaModeStepupComposition
            {
                GachaStepGroupId = entry.GachaId,
                StepNumber = 1,
                CurrentStepNumber = stepNumber,
                GachaAssetName = entry.BannerAssetName,
                CurrentLoopCount = loopCount
            }.ToByteString(),
            _ => new GachaModeBasic
            {
                GachaAssetName = entry.BannerAssetName
            }.ToByteString()
        };

        return gacha;
    }

    #endregion

    #region Draw Logic

    /// <summary>
    /// Draws from premium banner pools, using banner-specific pools when available.
    /// </summary>
    private List<DrawnItem> DrawPremium(GachaEntry entry, PricePhaseEntry phase, int count)
    {
        // Build banner-specific pools from catalog-filtered master data
        var (costumesByRarity, weaponsByRarity) = BuildBannerPools();

        // Rate tiers (out of 10000)
        (int Weight, int PossessionType, RarityType Rarity)[] rateTiers =
        [
            (200, (int)PossessionType.COSTUME, RarityType.SS_RARE),
            (300, (int)PossessionType.WEAPON, RarityType.SS_RARE),
            (500, (int)PossessionType.COSTUME, RarityType.S_RARE),
            (1000, (int)PossessionType.WEAPON, RarityType.S_RARE),
            (8000, (int)PossessionType.WEAPON, RarityType.RARE),
        ];

        int totalWeight = 0;
        foreach (var tier in rateTiers)
            totalWeight += tier.Weight;

        List<DrawnItem> result = new(count);
        for (int i = 0; i < count; i++)
        {
            bool isGuaranteeSlot = phase.FixedCount > 0 && i >= count - phase.FixedCount;
            DrawnItem item = RollOne(rateTiers, totalWeight, costumesByRarity, weaponsByRarity);

            if (isGuaranteeSlot && (int)item.Rarity < phase.FixedRarityMin)
            {
                item = RollAtMinRarity(rateTiers, (RarityType)phase.FixedRarityMin, costumesByRarity, weaponsByRarity);
            }

            result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// Draws random materials.
    /// </summary>
    private List<DrawnItem> DrawMaterials(int count)
    {
        List<EntityMMaterial> materials = _masterDb.EntityMMaterial;
        if (materials.Count == 0)
            return [];

        List<DrawnItem> result = new(count);
        for (int i = 0; i < count; i++)
        {
            EntityMMaterial mat = materials[Random.Shared.Next(materials.Count)];
            result.Add(new DrawnItem((int)PossessionType.MATERIAL, mat.MaterialId, mat.RarityType));
        }
        return result;
    }

    /// <summary>
    /// Builds costume and weapon pools from catalog-filtered master data.
    /// Only catalog items, exclude evolved/restricted weapons, only costumes SR+ and weapons R+.
    /// </summary>
    private (Dictionary<RarityType, List<PoolItem>> Costumes, Dictionary<RarityType, List<PoolItem>> Weapons) BuildBannerPools()
    {
        // Build catalog sets
        HashSet<int> catalogCostumes = [];
        foreach (EntityMCatalogCostume cc in _masterDb.EntityMCatalogCostume)
            catalogCostumes.Add(cc.CostumeId);

        HashSet<int> catalogWeapons = [];
        foreach (EntityMCatalogWeapon cw in _masterDb.EntityMCatalogWeapon)
            catalogWeapons.Add(cw.WeaponId);

        // Build evolved weapon exclusion set
        HashSet<int> evolvedWeapons = BuildEvolvedWeaponSet();

        Dictionary<RarityType, List<PoolItem>> costumesByRarity = [];
        foreach (EntityMCostume c in _masterDb.EntityMCostume)
        {
            if (!catalogCostumes.Contains(c.CostumeId))
                continue;
            if (c.RarityType < RarityType.S_RARE)
                continue;

            if (!costumesByRarity.TryGetValue(c.RarityType, out List<PoolItem>? list))
            {
                list = [];
                costumesByRarity[c.RarityType] = list;
            }
            list.Add(new PoolItem((int)PossessionType.COSTUME, c.CostumeId, c.RarityType, c.CharacterId));
        }

        Dictionary<RarityType, List<PoolItem>> weaponsByRarity = [];
        foreach (EntityMWeapon w in _masterDb.EntityMWeapon)
        {
            if (!catalogWeapons.Contains(w.WeaponId))
                continue;
            if (evolvedWeapons.Contains(w.WeaponId))
                continue;
            if (w.IsRestrictDiscard)
                continue;

            if (!weaponsByRarity.TryGetValue(w.RarityType, out List<PoolItem>? list))
            {
                list = [];
                weaponsByRarity[w.RarityType] = list;
            }
            list.Add(new PoolItem((int)PossessionType.WEAPON, w.WeaponId, w.RarityType));
        }

        // Fallback: if catalog-filtered pools are empty, use all master data (backwards compat)
        if (costumesByRarity.Count == 0 && weaponsByRarity.Count == 0)
        {
            foreach (EntityMCostume c in _masterDb.EntityMCostume)
            {
                if (c.RarityType < RarityType.S_RARE) continue;
                if (!costumesByRarity.TryGetValue(c.RarityType, out List<PoolItem>? list))
                {
                    list = [];
                    costumesByRarity[c.RarityType] = list;
                }
                list.Add(new PoolItem((int)PossessionType.COSTUME, c.CostumeId, c.RarityType, c.CharacterId));
            }
            foreach (EntityMWeapon w in _masterDb.EntityMWeapon)
            {
                if (!weaponsByRarity.TryGetValue(w.RarityType, out List<PoolItem>? list))
                {
                    list = [];
                    weaponsByRarity[w.RarityType] = list;
                }
                list.Add(new PoolItem((int)PossessionType.WEAPON, w.WeaponId, w.RarityType));
            }
        }

        return (costumesByRarity, weaponsByRarity);
    }

    /// <summary>
    /// Builds the set of evolved weapon IDs to exclude from the gacha pool.
    /// </summary>
    private HashSet<int> BuildEvolvedWeaponSet()
    {
        Dictionary<int, List<EntityMWeaponEvolutionGroup>> grouped = [];
        foreach (EntityMWeaponEvolutionGroup row in _masterDb.EntityMWeaponEvolutionGroup)
        {
            if (!grouped.TryGetValue(row.WeaponEvolutionGroupId, out List<EntityMWeaponEvolutionGroup>? list))
            {
                list = [];
                grouped[row.WeaponEvolutionGroupId] = list;
            }
            list.Add(row);
        }

        HashSet<int> evolved = [];
        foreach (List<EntityMWeaponEvolutionGroup> chain in grouped.Values)
        {
            chain.Sort((a, b) => a.EvolutionOrder.CompareTo(b.EvolutionOrder));
            for (int i = 1; i < chain.Count; i++)
                evolved.Add(chain[i].WeaponId);
        }
        return evolved;
    }

    /// <summary>
    /// Builds costume->weapon pairing map using catalog term matching.
    /// </summary>
    private Dictionary<int, int> BuildCostumeWeaponMap()
    {
        Dictionary<int, int> costumeTermId = [];
        HashSet<int> catalogCostumeSet = [];
        foreach (EntityMCatalogCostume cc in _masterDb.EntityMCatalogCostume)
        {
            catalogCostumeSet.Add(cc.CostumeId);
            costumeTermId[cc.CostumeId] = cc.CatalogTermId;
        }

        HashSet<int> catalogWeaponSet = [];
        foreach (EntityMCatalogWeapon cw in _masterDb.EntityMCatalogWeapon)
            catalogWeaponSet.Add(cw.WeaponId);

        HashSet<int> evolvedWeapons = BuildEvolvedWeaponSet();

        Dictionary<int, WeaponType> costumeWeaponType = [];
        Dictionary<int, RarityType> costumeRarity = [];
        foreach (EntityMCostume c in _masterDb.EntityMCostume)
        {
            if (!catalogCostumeSet.Contains(c.CostumeId) || c.RarityType < RarityType.S_RARE)
                continue;
            costumeWeaponType[c.CostumeId] = c.SkillfulWeaponType;
            costumeRarity[c.CostumeId] = c.RarityType;
        }

        Dictionary<int, WeaponType> weaponTypeById = [];
        Dictionary<int, RarityType> weaponRarityById = [];
        HashSet<int> restrictedWeapons = [];
        foreach (EntityMWeapon w in _masterDb.EntityMWeapon)
        {
            weaponTypeById[w.WeaponId] = w.WeaponType;
            weaponRarityById[w.WeaponId] = w.RarityType;
            if (w.IsRestrictDiscard)
                restrictedWeapons.Add(w.WeaponId);
        }

        // Group weapons by (termId, weaponType, rarity)
        Dictionary<(int TermId, WeaponType WType, RarityType Rarity), List<int>> weaponsByKey = [];
        foreach (EntityMCatalogWeapon cw in _masterDb.EntityMCatalogWeapon)
        {
            if (evolvedWeapons.Contains(cw.WeaponId) || restrictedWeapons.Contains(cw.WeaponId))
                continue;
            if (!weaponTypeById.TryGetValue(cw.WeaponId, out WeaponType wt) || wt == 0)
                continue;
            if (!weaponRarityById.TryGetValue(cw.WeaponId, out RarityType r) || r < RarityType.S_RARE)
                continue;

            var key = (cw.CatalogTermId, wt, r);
            if (!weaponsByKey.TryGetValue(key, out List<int>? list))
            {
                list = [];
                weaponsByKey[key] = list;
            }
            list.Add(cw.WeaponId);
        }

        foreach (List<int> ids in weaponsByKey.Values)
            ids.Sort();

        Dictionary<int, int> result = [];
        foreach (int costumeId in costumeWeaponType.Keys)
        {
            if (!costumeTermId.TryGetValue(costumeId, out int tid))
                continue;
            if (!costumeWeaponType.TryGetValue(costumeId, out WeaponType cwt))
                continue;
            if (!costumeRarity.TryGetValue(costumeId, out RarityType cr))
                continue;

            var key = (tid, cwt, cr);
            if (!weaponsByKey.TryGetValue(key, out List<int>? candidates) || candidates.Count == 0)
                continue;

            if (candidates.Count == 1)
            {
                result[costumeId] = candidates[0];
                continue;
            }

            // Try id-pattern match first
            int idPattern = costumeId * 10 + 1;
            bool found = false;
            foreach (int wid in candidates)
            {
                if (wid == idPattern)
                {
                    result[costumeId] = wid;
                    found = true;
                    break;
                }
            }
            if (!found)
                result[costumeId] = candidates[0];
        }

        return result;
    }

    #endregion

    #region Rolling

    private static DrawnItem RollOne(
        (int Weight, int PossessionType, RarityType Rarity)[] rateTiers,
        int totalWeight,
        Dictionary<RarityType, List<PoolItem>> costumesByRarity,
        Dictionary<RarityType, List<PoolItem>> weaponsByRarity)
    {
        int roll = Random.Shared.Next(totalWeight);
        int cumulative = 0;
        int tierPossessionType = (int)PossessionType.WEAPON;
        RarityType tierRarity = RarityType.RARE;

        foreach (var tier in rateTiers)
        {
            cumulative += tier.Weight;
            if (roll < cumulative)
            {
                tierPossessionType = tier.PossessionType;
                tierRarity = tier.Rarity;
                break;
            }
        }

        return PickFromPool(tierPossessionType, tierRarity, costumesByRarity, weaponsByRarity);
    }

    private static DrawnItem RollAtMinRarity(
        (int Weight, int PossessionType, RarityType Rarity)[] rateTiers,
        RarityType minRarity,
        Dictionary<RarityType, List<PoolItem>> costumesByRarity,
        Dictionary<RarityType, List<PoolItem>> weaponsByRarity)
    {
        int filteredTotal = 0;
        List<(int Weight, int PossessionType, RarityType Rarity)> filtered = [];
        foreach (var tier in rateTiers)
        {
            if (tier.Rarity >= minRarity)
            {
                filtered.Add(tier);
                filteredTotal += tier.Weight;
            }
        }

        if (filteredTotal == 0)
        {
            return PickFromPool((int)PossessionType.WEAPON, minRarity, costumesByRarity, weaponsByRarity);
        }

        int roll = Random.Shared.Next(filteredTotal);
        int cumulative = 0;
        int tierPossessionType = (int)PossessionType.WEAPON;
        RarityType tierRarity = minRarity;

        foreach (var tier in filtered)
        {
            cumulative += tier.Weight;
            if (roll < cumulative)
            {
                tierPossessionType = tier.PossessionType;
                tierRarity = tier.Rarity;
                break;
            }
        }

        return PickFromPool(tierPossessionType, tierRarity, costumesByRarity, weaponsByRarity);
    }

    private static DrawnItem PickFromPool(
        int possessionType,
        RarityType rarity,
        Dictionary<RarityType, List<PoolItem>> costumesByRarity,
        Dictionary<RarityType, List<PoolItem>> weaponsByRarity)
    {
        if (possessionType == (int)PossessionType.COSTUME)
        {
            List<PoolItem>? pool = costumesByRarity.GetValueOrDefault(rarity);
            if (pool == null || pool.Count == 0)
                pool = costumesByRarity.GetValueOrDefault(RarityType.SS_RARE);
            if (pool == null || pool.Count == 0)
                return PickFromPool((int)PossessionType.WEAPON, rarity, costumesByRarity, weaponsByRarity);

            PoolItem pick = pool[Random.Shared.Next(pool.Count)];
            return new DrawnItem(pick.PossessionType, pick.PossessionId, pick.Rarity, pick.CharacterId);
        }

        // Weapon
        List<PoolItem>? weaponPool = weaponsByRarity.GetValueOrDefault(rarity);
        if (weaponPool == null || weaponPool.Count == 0)
            weaponPool = weaponsByRarity.GetValueOrDefault(RarityType.RARE);
        if (weaponPool == null || weaponPool.Count == 0)
            return new DrawnItem((int)PossessionType.WEAPON, 0, rarity);

        PoolItem weaponPick = weaponPool[Random.Shared.Next(weaponPool.Count)];
        return new DrawnItem(weaponPick.PossessionType, weaponPick.PossessionId, weaponPick.Rarity);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Finds the matching price phase or falls back to the first available.
    /// </summary>
    private static PricePhaseEntry FindPhase(GachaEntry entry, int phaseId)
    {
        foreach (PricePhaseEntry p in entry.PricePhases)
        {
            if (p.PhaseId == phaseId)
                return p;
        }
        if (entry.PricePhases.Count > 0)
            return entry.PricePhases[0];

        return new PricePhaseEntry
        {
            PriceType = (int)PriceType.GEM,
            Price = PremiumMultiPullPrice,
            DrawCount = PremiumMultiPullCount
        };
    }

    /// <summary>
    /// Deducts currency based on price type.
    /// </summary>
    private static void DeductPrice(DarkUserMemoryDatabase userDb, int priceType, int priceId, int amount)
    {
        switch ((PriceType)priceType)
        {
            case PriceType.CONSUMABLE_ITEM:
                {
                    EntityIUserConsumableItem? item = userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == priceId);
                    if (item != null)
                        item.Count = Math.Max(0, item.Count - amount);
                    break;
                }
            case PriceType.GEM:
                {
                    EntityIUserGem? gem = userDb.EntityIUserGem.FirstOrDefault();
                    if (gem == null) break;
                    if (gem.FreeGem >= amount)
                    {
                        gem.FreeGem -= amount;
                    }
                    else
                    {
                        int remaining = amount - gem.FreeGem;
                        gem.FreeGem = 0;
                        gem.PaidGem -= remaining;
                    }
                    break;
                }
            case PriceType.PAID_GEM:
                {
                    EntityIUserGem? gem = userDb.EntityIUserGem.FirstOrDefault();
                    if (gem != null)
                        gem.PaidGem -= amount;
                    break;
                }
            case PriceType.PLATFORM_PAYMENT:
                // Real-money purchase — treat as free on private server
                break;
        }
    }

    private static EntitySGachaBannerState? FindBannerState(DarkUserMemoryDatabase userDb, int gachaId)
    {
        foreach (EntitySGachaBannerState bs in userDb.EntitySGachaBannerState)
        {
            if (bs.GachaId == gachaId)
                return bs;
        }
        return null;
    }

    private static EntitySGachaBannerState GetOrCreateBannerState(DarkUserMemoryDatabase userDb, int gachaId)
    {
        EntitySGachaBannerState? bs = FindBannerState(userDb, gachaId);
        if (bs != null) return bs;

        bs = new EntitySGachaBannerState
        {
            UserId = userDb.UserId,
            GachaId = gachaId,
            BoxNumber = 1,
            StepNumber = 1
        };
        userDb.EntitySGachaBannerState.Add(bs);
        return bs;
    }

    private static long StartOfDayMillis()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset startOfDay = new(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        return startOfDay.ToUnixTimeMilliseconds();
    }

    private static bool IsOwnedByType(
        int possessionType,
        int possessionId,
        HashSet<int> ownedCostumes,
        HashSet<int> ownedWeapons,
        DarkUserMemoryDatabase userDb)
    {
        return possessionType switch
        {
            (int)PossessionType.COSTUME => ownedCostumes.Contains(possessionId),
            (int)PossessionType.WEAPON => ownedWeapons.Contains(possessionId),
            (int)PossessionType.MATERIAL => userDb.EntityIUserMaterial.Any(m => m.MaterialId == possessionId && m.Count > 0),
            (int)PossessionType.WEAPON_ENHANCED => userDb.EntityIUserConsumableItem.Any(c => c.ConsumableItemId == possessionId && c.Count > 0),
            _ => false
        };
    }

    #endregion

    #region Types

    private sealed record GachaEntry
    {
        public int GachaId { get; init; }
        public GachaLabelType LabelType { get; init; }
        public GachaModeType ModeType { get; init; }
        public long StartDatetime { get; init; }
        public long EndDatetime { get; init; }
        public EntityMGachaMedal? Medal { get; init; }
        public string BannerAssetName { get; init; } = string.Empty;
        public int SortOrder { get; init; }
        public GachaDecorationType DecorationType { get; init; } = GachaDecorationType.NORMAL;
        public int RelatedMainQuestChapterId { get; init; }
        public int DescriptionTextId { get; init; }
        public List<PricePhaseEntry> PricePhases { get; init; } = [];
        public int MaxStepNumber { get; init; }
    }

    private sealed class PricePhaseEntry
    {
        public int PhaseId { get; init; }
        public int PriceType { get; init; }
        public int PriceId { get; init; }
        public int Price { get; init; }
        public int RegularPrice { get; init; }
        public int DrawCount { get; init; }
        public int FixedRarityMin { get; init; }
        public int FixedCount { get; init; }
        public int LimitExecCount { get; init; }
        public int StepNumber { get; init; }
    }

    private readonly record struct PoolItem(int PossessionType, int PossessionId, RarityType Rarity, int CharacterId = 0);

    private readonly record struct DrawnItem(int PossessionType, int PossessionId, RarityType Rarity, int CharacterId = 0);

    #endregion
}

