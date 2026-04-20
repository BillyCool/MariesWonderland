using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Banner;

namespace MariesWonderland.Services;

public class BannerService(DarkMasterMemoryDatabase masterDb) : MariesWonderland.Proto.Banner.BannerService.BannerServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Returns available gacha banners grouped into term-limited and chapter categories for the Mama banner screen.</summary>
    public override Task<GetMamaBannerResponse> GetMamaBanner(GetMamaBannerRequest request, ServerCallContext context)
    {
        Dictionary<int, EntityMGachaMedal> medalByGachaId = [];
        foreach (EntityMGachaMedal medal in _masterDb.EntityMGachaMedal)
        {
            medalByGachaId[medal.ShopTransitionGachaId] = medal;
        }

        List<GachaBanner> termLimited = [];
        HashSet<int> seenStepUpGroups = [];
        GachaBanner? latestChapter = null;

        foreach (EntityMMomBanner banner in _masterDb.EntityMMomBanner)
        {
            if (banner.DestinationDomainType != DomainType.GACHA)
            {
                continue;
            }

            int gachaId = banner.DestinationDomainId;

            GachaLabelType labelType = InferGachaLabelType(banner.BannerAssetName);

            // Chapter gachas (common_ prefix) are exempt from the medal requirement.
            if (!medalByGachaId.ContainsKey(gachaId) && labelType != GachaLabelType.CHAPTER)
            {
                continue;
            }

            // Skip portal cage and recycle banners (not displayed on home screen).
            if (labelType is GachaLabelType.PORTAL_CAGE or GachaLabelType.RECYCLE)
            {
                continue;
            }

            // Step-up banners: truncate to group ID (562001 → 562) using StepUpGroupDivisor.
            // Multiple raw step IDs map to the same group, so deduplicate.
            if (banner.BannerAssetName.StartsWith("step_up_", StringComparison.Ordinal))
            {
                gachaId /= 1000;
                if (!seenStepUpGroups.Add(gachaId))
                {
                    continue;
                }
            }

            GachaBanner b = new()
            {
                GachaLabelType = (int)labelType,
                GachaAssetName = banner.BannerAssetName,
                GachaId = gachaId
            };

            switch (labelType)
            {
                case GachaLabelType.EVENT:
                case GachaLabelType.PREMIUM:
                    termLimited.Add(b);
                    break;
                case GachaLabelType.CHAPTER:
                    if (latestChapter is null || gachaId > latestChapter.GachaId)
                    {
                        latestChapter = b;
                    }
                    break;
            }
        }

        GetMamaBannerResponse response = new()
        {
            LatestChapterGacha = latestChapter,
            IsExistUnreadPop = false
        };
        response.TermLimitedGacha.AddRange(termLimited);

        return Task.FromResult(response);
    }

    /// <summary>
    /// Master data has no GachaLabelType field, so we infer from the banner asset name prefix.
    /// All term-limited gachas (step_up_, limited_, etc.) use PREMIUM (value 1).
    /// </summary>
    private static GachaLabelType InferGachaLabelType(string assetName)
    {
        if (assetName.StartsWith("common_", StringComparison.Ordinal))
        {
            return GachaLabelType.CHAPTER;
        }
        if (assetName.StartsWith("portal_cage_", StringComparison.Ordinal))
        {
            return GachaLabelType.PORTAL_CAGE;
        }
        if (assetName.StartsWith("recycle_", StringComparison.Ordinal))
        {
            return GachaLabelType.RECYCLE;
        }
        // All term-limited gachas (step_up_, limited_, event_, etc.) default to PREMIUM (value 1).
        return GachaLabelType.PREMIUM;
    }
}

