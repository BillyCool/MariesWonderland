using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Banner;
using MariesWonderland.Summons;

namespace MariesWonderland.Services;

public class BannerService(UserDataStore store, DarkMasterMemoryDatabase masterDb, MissionTracker missions, BoxSummonTable boxSummons, TimeProvider timeProvider) : MariesWonderland.Proto.Banner.BannerService.BannerServiceBase
{
    private const int StepUpGroupDivisor = 1000;

    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly MissionTracker _missions = missions;
    private readonly BoxSummonTable _boxSummons = boxSummons;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// The summons on Mama's banner, matching the summon list: premium banners (one per step-up group) and Mama's
    /// Dream box summons as term-limited, and the chapter summon of the furthest main chapter the user has reached.
    /// Also takes the Cage values measured since the last report.
    /// </summary>
    public override Task<GetMamaBannerResponse> GetMamaBanner(GetMamaBannerRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        if (request.CageMeasurableValues is { } cage)
        {
            _missions.RecordCageValues(userDb, cage.RunningDistanceMeters, cage.MamaTappedCount, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());
        }

        HashSet<int> medalGachaIds = [.. _masterDb.EntityMGachaMedal.Select(m => m.ShopTransitionGachaId)];
        List<GachaBanner> termLimited = [];
        HashSet<int> seenStepUpGroups = [];
        foreach (EntityMMomBanner banner in _masterDb.EntityMMomBanner)
        {
            if (banner.DestinationDomainType != DomainType.GACHA || _boxSummons.Find(banner.DestinationDomainId) != null
                || !medalGachaIds.Contains(banner.DestinationDomainId)
                || banner.BannerAssetName.StartsWith("portal_cage_", StringComparison.Ordinal)
                || banner.BannerAssetName.StartsWith("recycle_", StringComparison.Ordinal))
            {
                continue;
            }

            int gachaId = banner.DestinationDomainId;
            if (banner.BannerAssetName.StartsWith("step_up_", StringComparison.Ordinal))
            {
                gachaId /= StepUpGroupDivisor;
                if (!seenStepUpGroups.Add(gachaId))
                {
                    continue;
                }
            }

            termLimited.Add(new GachaBanner { GachaLabelType = (int)GachaLabelType.PREMIUM, GachaAssetName = banner.BannerAssetName, GachaId = gachaId });
        }

        foreach (BoxSummon summon in _boxSummons.All.Where(b => b.Kind == BoxSummonKind.MAMAS_DREAM).OrderBy(b => b.GachaId))
        {
            termLimited.Add(new GachaBanner { GachaLabelType = (int)GachaLabelType.EVENT, GachaAssetName = summon.AssetName, GachaId = summon.GachaId });
        }

        BoxSummon? latestChapter = _boxSummons.All
            .Where(b => b.Kind == BoxSummonKind.CHAPTER && b.MainQuestChapterId > 0 && BoxSummonTable.IsChapterReached(userDb, _masterDb, b))
            .MaxBy(b => b.MainQuestChapterId);

        GetMamaBannerResponse response = new()
        {
            LatestChapterGacha = latestChapter == null
                ? null
                : new GachaBanner { GachaLabelType = (int)GachaLabelType.CHAPTER, GachaAssetName = latestChapter.AssetName, GachaId = latestChapter.GachaId },
            IsExistUnreadPop = false
        };
        response.TermLimitedGacha.AddRange(termLimited);

        return Task.FromResult(response);
    }
}
