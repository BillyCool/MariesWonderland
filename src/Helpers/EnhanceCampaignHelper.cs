using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Enhance campaigns: they raise the memoir success rate and the great success chance of costume and weapon enhances.
/// Values are permil (400 = 40 percentage points). The best active campaign wins: an additional campaign adds its value
/// to the base rate, a probability campaign sets the rate to its value. Campaigns for everyone are active between their
/// start and end; beginner and comeback campaigns are active for a user whose beginner or comeback date (their
/// <c>IUserBeginnerCampaign</c> / <c>IUserComebackCampaign</c> row) falls inside the campaign, until that user's own
/// campaign term ends.
/// </summary>
public sealed class EnhanceCampaignHelper
{
    /// <summary>Chance, before campaigns, that a costume or weapon enhance is a great success (permil).</summary>
    public const int GreatSuccessChancePermil = 200;

    private const long DayMs = 24L * 60 * 60 * 1000;

    private readonly IReadOnlyList<EnhanceCampaignRow> _campaigns;
    private readonly IReadOnlyDictionary<int, EntityMBeginnerCampaign> _beginnerCampaigns;
    private readonly IReadOnlyDictionary<int, EntityMComebackCampaign> _comebackCampaigns;
    private readonly Random _random;

    public EnhanceCampaignHelper(DarkMasterMemoryDatabase masterDb, Random? random = null)
    {
        _random = random ?? Random.Shared;

        Dictionary<int, List<EntityMEnhanceCampaignTargetGroup>> targetsByGroupId = masterDb.EntityMEnhanceCampaignTargetGroup
            .GroupBy(t => t.EnhanceCampaignTargetGroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<EnhanceCampaignRow> campaigns = [];
        foreach (EntityMEnhanceCampaign campaign in masterDb.EntityMEnhanceCampaign)
        {
            if (targetsByGroupId.TryGetValue(campaign.EnhanceCampaignTargetGroupId, out List<EntityMEnhanceCampaignTargetGroup>? targets))
            {
                campaigns.Add(new EnhanceCampaignRow(campaign, targets));
            }
        }

        _campaigns = campaigns;
        _beginnerCampaigns = masterDb.EntityMBeginnerCampaign.ToDictionary(c => c.BeginnerCampaignId);
        _comebackCampaigns = masterDb.EntityMComebackCampaign.ToDictionary(c => c.ComebackCampaignId);
    }

    /// <summary>Rolls whether a costume enhance is a great success, with the best active campaign for it applied.</summary>
    public bool RollCostumeGreatSuccess(EntityMCostume costume, DarkUserMemoryDatabase userDb, long nowMs)
        => RollGreatSuccess(userDb, nowMs, t => MatchesCostume(t, costume));

    /// <summary>Rolls whether a weapon enhance is a great success, with the best active campaign for it applied.</summary>
    public bool RollWeaponGreatSuccess(EntityMWeapon weapon, DarkUserMemoryDatabase userDb, long nowMs)
        => RollGreatSuccess(userDb, nowMs, t => MatchesWeapon(t, weapon));

    /// <summary>What a great success gives back of a cost: half, rounded down.</summary>
    public static int GreatSuccessRefund(int cost) => cost / 2;

    /// <summary>Memoir enhance success rate (permil) with the best active campaign for the memoir applied, capped at 1000.</summary>
    public int ApplyPartsSuccessRate(int ratePermil, EntityMParts parts, DarkUserMemoryDatabase userDb, long nowMs)
    {
        bool Matches(EntityMEnhanceCampaignTargetGroup t) => t.EnhanceCampaignTargetType switch
        {
            EnhanceCampaignTargetType.PARTS_ALL => true,
            EnhanceCampaignTargetType.PARTS_SERIES_ID => t.EnhanceCampaignTargetValue == parts.PartsGroupId,
            EnhanceCampaignTargetType.PARTS_ID => t.EnhanceCampaignTargetValue == parts.PartsId,
            _ => false
        };

        return RatePermil(ratePermil, userDb, nowMs, Matches);
    }

    private bool RollGreatSuccess(DarkUserMemoryDatabase userDb, long nowMs, Func<EntityMEnhanceCampaignTargetGroup, bool> matches)
        => _random.Next(1000) < RatePermil(GreatSuccessChancePermil, userDb, nowMs, matches);

    /// <summary>
    /// The rate with campaigns applied: the best of every active matching campaign (an additional one adds to
    /// <paramref name="basePermil"/>, a probability one replaces it), or the base when none is active. At most 1000.
    /// </summary>
    private int RatePermil(int basePermil, DarkUserMemoryDatabase userDb, long nowMs, Func<EntityMEnhanceCampaignTargetGroup, bool> matches)
    {
        int? best = null;
        foreach (EnhanceCampaignRow row in _campaigns)
        {
            EntityMEnhanceCampaign campaign = row.Campaign;
            int? rate = campaign.EnhanceCampaignEffectType switch
            {
                EnhanceCampaignEffectType.ADDITIONAL_PER_MILE => basePermil + campaign.EnhanceCampaignEffectValue,
                EnhanceCampaignEffectType.PROBABILITY_PER_MILE => campaign.EnhanceCampaignEffectValue,
                _ => null
            };

            if (rate is null || !IsActive(campaign, userDb, nowMs) || !row.Targets.Any(matches))
            {
                continue;
            }

            best = best is null ? rate : Math.Max(best.Value, rate.Value);
        }

        return Math.Min(1000, best ?? basePermil);
    }

    private bool IsActive(EntityMEnhanceCampaign campaign, DarkUserMemoryDatabase userDb, long nowMs)
    {
        switch (campaign.TargetUserStatusType)
        {
            case TargetUserStatusType.ALL:
                return nowMs >= campaign.StartDatetime && nowMs <= campaign.EndDatetime;

            case TargetUserStatusType.BEGINNER:
            {
                EntityIUserBeginnerCampaign? beginner = userDb.EntityIUserBeginnerCampaign.FirstOrDefault();
                return beginner != null
                    && _beginnerCampaigns.TryGetValue(beginner.BeginnerCampaignId, out EntityMBeginnerCampaign? term)
                    && IsUserCampaignActive(campaign, beginner.CampaignRegisterDatetime, term.GrantCampaignTermDayCount, nowMs);
            }

            case TargetUserStatusType.COMEBACK:
            {
                EntityIUserComebackCampaign? comeback = userDb.EntityIUserComebackCampaign.FirstOrDefault();
                return comeback != null
                    && _comebackCampaigns.TryGetValue(comeback.ComebackCampaignId, out EntityMComebackCampaign? term)
                    && IsUserCampaignActive(campaign, comeback.ComebackDatetime, term.GrantCampaignTermDayCount, nowMs);
            }

            default:
                // COMEBACK_GRADE needs comeback grades, which aren't modelled.
                return false;
        }
    }

    /// <summary>
    /// A beginner or comeback campaign applies when the user's date for it is inside the campaign and the user's own
    /// term (that date plus the term days) hasn't ended.
    /// </summary>
    private static bool IsUserCampaignActive(EntityMEnhanceCampaign campaign, long userDatetime, int termDays, long nowMs)
        => userDatetime >= campaign.StartDatetime && userDatetime <= campaign.EndDatetime
            && nowMs >= userDatetime && nowMs <= userDatetime + termDays * DayMs;

    private static bool MatchesCostume(EntityMEnhanceCampaignTargetGroup t, EntityMCostume costume) => t.EnhanceCampaignTargetType switch
    {
        EnhanceCampaignTargetType.COSTUME_ALL => true,
        EnhanceCampaignTargetType.COSTUME_CHARACTER_ID => t.EnhanceCampaignTargetValue == costume.CharacterId,
        EnhanceCampaignTargetType.COSTUME_SKILLFUL_WEAPON_TYPE_ID => t.EnhanceCampaignTargetValue == (int)costume.SkillfulWeaponType,
        EnhanceCampaignTargetType.COSTUME_ID => t.EnhanceCampaignTargetValue == costume.CostumeId,
        _ => false
    };

    private static bool MatchesWeapon(EntityMEnhanceCampaignTargetGroup t, EntityMWeapon weapon) => t.EnhanceCampaignTargetType switch
    {
        EnhanceCampaignTargetType.WEAPON_ALL => true,
        EnhanceCampaignTargetType.WEAPON_TYPE_ID => t.EnhanceCampaignTargetValue == (int)weapon.WeaponType,
        EnhanceCampaignTargetType.WEAPON_ATTRIBUTE_TYPE_ID => t.EnhanceCampaignTargetValue == (int)weapon.AttributeType,
        EnhanceCampaignTargetType.WEAPON_ID => t.EnhanceCampaignTargetValue == weapon.WeaponId,
        _ => false
    };

    private sealed record EnhanceCampaignRow(EntityMEnhanceCampaign Campaign, IReadOnlyList<EntityMEnhanceCampaignTargetGroup> Targets);
}
