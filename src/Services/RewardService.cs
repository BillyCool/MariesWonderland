using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Arena;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.BigHunt;
using MariesWonderland.Proto.Labyrinth;
using MariesWonderland.Proto.Pvp;
using MariesWonderland.Proto.Reward;
using MariesWonderland.Quests;

namespace MariesWonderland.Services;

public class RewardService(UserDataStore store, DarkMasterMemoryDatabase masterDb, TimeProvider timeProvider)
    : MariesWonderland.Proto.Reward.RewardService.RewardServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Gift text 30: "Obtained as a weekly reward from Subjugation Battles."</summary>
    private const int BigHuntWeeklyGiftTextId = 30;

    /// <summary>
    /// Sends last week's score rewards to the gift box once: per attribute, the highest tier its weekly max score reached.
    /// Important items ("Daily Reward+") do not boost them.
    /// </summary>
    public override Task<ReceiveBigHuntRewardResponse> ReceiveBigHuntReward(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        BigHuntRewardHelper.EnsureWeek(_masterDb, userDb, userId, nowMs);

        // The reward is for the week that just ended (a week with no score has nothing to claim).
        long claimVersion = BigHuntRewardHelper.ClaimWeekVersion(nowMs);
        EntityIUserBigHuntWeeklyStatus? ws = userDb.EntityIUserBigHuntWeeklyStatus
            .FirstOrDefault(s => s.BigHuntWeeklyVersion == claimVersion);

        bool isReceived = BigHuntRewardHelper.IsWeekSettled(_masterDb, userDb, claimVersion, nowMs);

        List<WeeklyScoreResult> weeklyScoreResults = [];
        List<BigHuntReward> weeklyRewards = [];

        foreach (EntityMBigHuntBoss boss in _masterDb.EntityMBigHuntBoss)
        {
            long maxScore = BigHuntRewardHelper.WeekScore(_masterDb, userDb, claimVersion, boss, nowMs);
            int gradeIcon = ResolveGradeIconId(boss.BigHuntBossId, maxScore);

            weeklyScoreResults.Add(new WeeklyScoreResult
            {
                AttributeType = (int)boss.AttributeType,
                BeforeMaxScore = maxScore,
                CurrentMaxScore = maxScore,
                BeforeAssetGradeIconId = gradeIcon,
                CurrentAssetGradeIconId = gradeIcon,
                AfterMaxScore = maxScore,
                AfterAssetGradeIconId = gradeIcon,
            });
        }

        if (!isReceived)
        {
            foreach (EntityMBigHuntBoss boss in _masterDb.EntityMBigHuntBoss)
            {
                int rewardGroupId = BigHuntRewardHelper.ResolveWeeklyRewardGroupId(_masterDb, boss.AttributeType, nowMs);
                if (rewardGroupId == 0) { continue; }

                long maxScore = BigHuntRewardHelper.WeekScore(_masterDb, userDb, claimVersion, boss, nowMs);

                foreach (EntityMBigHuntRewardGroup item in BigHuntRewardHelper.HighestTierItems(_masterDb, rewardGroupId, maxScore))
                {
                    weeklyRewards.Add(new BigHuntReward
                    {
                        PossessionType = (int)item.PossessionType,
                        PossessionId = item.PossessionId,
                        Count = item.Count,
                    });
                }
            }

            // The rewards go to the gift box, one gift per possession ("Obtained as a weekly reward from Subjugation Battles.").
            foreach (BigHuntReward reward in BigHuntRewardHelper.Combine(weeklyRewards))
            {
                userDb.EntitySUserGift.Add(new EntitySUserGift
                {
                    UserId = userId,
                    UserGiftUuid = Guid.NewGuid().ToString(),
                    PossessionType = reward.PossessionType,
                    PossessionId = reward.PossessionId,
                    Count = reward.Count,
                    GrantDatetime = nowMs,
                    DescriptionGiftTextId = BigHuntWeeklyGiftTextId,
                    ExpirationDatetime = nowMs + GameConfig.GiftExpirationDays * 24L * 60 * 60 * 1000
                });
            }

            if (ws == null)
            {
                ws = new EntityIUserBigHuntWeeklyStatus
                {
                    UserId = userId,
                    BigHuntWeeklyVersion = claimVersion,
                };
                userDb.EntityIUserBigHuntWeeklyStatus.Add(ws);
            }
            ws.IsReceivedWeeklyReward = true;
            isReceived = true;
        }

        ReceiveBigHuntRewardResponse response = new()
        {
            IsReceivedWeeklyScoreReward = isReceived,
        };
        response.WeeklyScoreResult.AddRange(weeklyScoreResults);
        List<BigHuntReward> combinedRewards = BigHuntRewardHelper.Combine(weeklyRewards);
        response.WeeklyScoreReward.AddRange(combinedRewards);

        // The client's weekly result dialog lists LastWeekWeeklyScoreReward (the week just paid), not WeeklyScoreReward.
        response.LastWeekWeeklyScoreReward.AddRange(combinedRewards);
        return Task.FromResult(response);
    }

    /// <summary>
    /// Pays the Arena rewards of finished weeks (grade and place) and ended seasons (place) not paid yet. The player's
    /// weekly rows are brought up to date from the Arena first, so a week that ended has its final points and place.
    /// </summary>
    public override Task<ReceivePvpRewardResponse> ReceivePvpReward(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase? userDb))
        {
            return Task.FromResult(new ReceivePvpRewardResponse());
        }

        long syncedAt = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        ArenaHelper.SyncWeeks(_store, userDb, syncedAt);

        EntityIUserPvpStatus? existingStatus = userDb.EntityIUserPvpStatus.FirstOrDefault(s => s.UserId == userId);
        long latestReceivedWeeklyVersion = existingStatus?.LatestRewardReceivePvpWeeklyVersion ?? 0;
        int latestReceivedSeasonId = existingStatus?.LatestRewardReceivePvpSeasonId ?? 0;
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        // Only finished weeks pay: the client claims the newest finished week, and the week still running is paid next week.
        const long WeekMs = 7L * 24 * 60 * 60 * 1000;
        ReceivePvpRewardResponse response = new();

        long newestClaimedWeeklyVersion = latestReceivedWeeklyVersion;
        foreach (EntityIUserPvpWeeklyResult weeklyResult in userDb.EntityIUserPvpWeeklyResult
            .Where(r => r.UserId == userId && r.PvpWeeklyVersion > latestReceivedWeeklyVersion && r.PvpWeeklyVersion + WeekMs <= nowMs)
            .OrderBy(r => r.PvpWeeklyVersion))
        {
            EntityMPvpSeason? season = _masterDb.EntityMPvpSeason.FirstOrDefault(s => s.PvpSeasonId == weeklyResult.PvpSeasonId);
            if (season is null)
            {
                break;
            }

            int gradeRewardGroupId = ResolvePvpGradeWeeklyRewardGroupId(season, weeklyResult.FinalPoint);
            int weeklyRankRewardGroupId = ResolvePvpWeeklyRankRewardGroupId(season, weeklyResult.FinalRank);

            ApplyPvpRewards(userDb, CollectPvpRewards(
                gradeRewardGroupId,
                _masterDb.EntityMPvpGradeWeeklyRewardGroup,
                g => g.PvpGradeWeeklyRewardGroupId,
                g => g.PvpRewardId,
                g => g.SortOrder), nowMs);
            ApplyPvpRewards(userDb, CollectPvpRewards(
                weeklyRankRewardGroupId,
                _masterDb.EntityMPvpWeeklyRankRewardGroup,
                g => g.PvpWeeklyRankRewardGroupId,
                g => g.PvpRewardId,
                g => g.SortOrder), nowMs);

            response.WeeklyGradeResult = new WeeklyGradeResult
            {
                TargetSeasonId = weeklyResult.PvpSeasonId,
                PvpPoint = weeklyResult.FinalPoint,
                PvpGradeWeeklyRewardGroupId = gradeRewardGroupId
            };
            response.WeeklyRankResult = new WeeklyRankResult
            {
                TargetSeasonId = weeklyResult.PvpSeasonId,
                Rank = weeklyResult.FinalRank,
                PvpWeeklyRankRewardGroupId = weeklyRankRewardGroupId
            };

            newestClaimedWeeklyVersion = weeklyResult.PvpWeeklyVersion;
        }

        int newestClaimedSeasonId = latestReceivedSeasonId;
        foreach (IGrouping<int, EntityIUserPvpWeeklyResult> seasonResults in userDb.EntityIUserPvpWeeklyResult
            .Where(r => r.UserId == userId && r.PvpSeasonId > latestReceivedSeasonId)
            .GroupBy(r => r.PvpSeasonId)
            .OrderBy(g => g.Key))
        {
            EntityIUserPvpWeeklyResult latestSeasonResult = seasonResults
                .OrderBy(r => r.PvpWeeklyVersion)
                .Last();
            EntityMPvpSeason? season = _masterDb.EntityMPvpSeason.FirstOrDefault(s => s.PvpSeasonId == latestSeasonResult.PvpSeasonId);
            if (season is null)
            {
                break;
            }

            if (season.SeasonEndDatetime > nowMs)
            {
                break;
            }

            // The place when the season ended, as the Arena recorded it (0, and no reward, when it has no record of
            // the player in that season). The season still counts as claimed either way.
            int seasonRank = _store.Arena.Standing(season.PvpSeasonId, userId, nowMs)?.SeasonFinalRank ?? 0;

            int seasonRankRewardGroupId = ResolvePvpSeasonRankRewardGroupId(season, seasonRank);
            ApplyPvpRewards(userDb, CollectPvpRewards(
                seasonRankRewardGroupId,
                _masterDb.EntityMPvpSeasonRankRewardGroup,
                g => g.PvpSeasonRankRewardGroupId,
                g => g.PvpRewardId,
                g => g.SortOrder), nowMs);

            response.SeasonRankResult = new SeasonRankResult
            {
                TargetSeasonId = latestSeasonResult.PvpSeasonId,
                Rank = seasonRank,
                PvpSeasonRankRewardGroupId = seasonRankRewardGroupId
            };

            newestClaimedSeasonId = latestSeasonResult.PvpSeasonId;
        }

        if (newestClaimedWeeklyVersion == latestReceivedWeeklyVersion
            && newestClaimedSeasonId == latestReceivedSeasonId)
        {
            return Task.FromResult(response);
        }

        EntityIUserPvpStatus status = existingStatus ?? userDb.EntityIUserPvpStatus.GetOrCreate(userId);
        status.LatestRewardReceivePvpWeeklyVersion = newestClaimedWeeklyVersion;
        status.LatestRewardReceivePvpSeasonId = newestClaimedSeasonId;

        return Task.FromResult(response);
    }

    /// <summary>
    /// The home screen's Fate Board season results: pays every board whose last joined season has ended and isn't
    /// paid yet (the client asks within 30 days of a season's end).
    /// </summary>
    public override Task<ReceiveLabyrinthSeasonRewardResponse> ReceiveLabyrinthSeasonReward(Empty request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        ReceiveLabyrinthSeasonRewardResponse response = new();
        foreach (EntityIUserEventQuestLabyrinthSeason row in userDb.EntityIUserEventQuestLabyrinthSeason.OrderBy(s => s.EventQuestChapterId))
        {
            if (FateBoardHelper.PayEndedSeason(userDb, _masterDb, row, nowMs) is { } result)
            {
                response.SeasonResult.Add(result);
            }
        }

        return Task.FromResult(response);
    }

    /// <summary>
    /// Claims what's left on the most recently ended mission pass that still has unclaimed levels, and returns its
    /// id (0 when there's nothing left on any ended pass).
    /// </summary>
    public override Task<ReceiveMissionPassRemainingRewardResponse> ReceiveMissionPassRemainingReward(Empty request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        int missionPassId = MissionPassHelper.ClaimRemaining(userDb, _masterDb, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());
        return Task.FromResult(new ReceiveMissionPassRemainingRewardResponse { RewardReceivedMissionPassId = missionPassId });
    }

    private int ResolveGradeIconId(int bossId, long score)
    {
        EntityMBigHuntBoss? boss = _masterDb.EntityMBigHuntBoss.FirstOrDefault(b => b.BigHuntBossId == bossId);
        if (boss == null) { return 0; }

        List<EntityMBigHuntBossGradeGroup> thresholds = [.. _masterDb.EntityMBigHuntBossGradeGroup
            .Where(g => g.BigHuntBossGradeGroupId == boss.BigHuntBossGradeGroupId)
            .OrderBy(g => g.NecessaryScore)];

        int iconId = 0;
        foreach (EntityMBigHuntBossGradeGroup t in thresholds)
        {
            if (score >= t.NecessaryScore) { iconId = t.AssetGradeIconId; }
            else { break; }
        }
        return iconId;
    }

    private int ResolvePvpGradeWeeklyRewardGroupId(EntityMPvpSeason season, int finalPoint)
    {
        List<EntityMPvpSeasonGrade> seasonGrades = [.. _masterDb.EntityMPvpSeasonGrade
            .Where(g => g.PvpSeasonId == season.PvpSeasonId)
            .OrderBy(g => g.NecessaryPvpPoint)];
        if (seasonGrades.Count > 0)
        {
            int rewardGroupId = 0;
            foreach (EntityMPvpSeasonGrade grade in seasonGrades)
            {
                if (finalPoint >= grade.NecessaryPvpPoint)
                {
                    rewardGroupId = grade.PvpGradeWeeklyRewardGroupId;
                }
                else
                {
                    break;
                }
            }
            return rewardGroupId;
        }

        List<EntityMPvpGradeGroup> gradeGroups = [.. _masterDb.EntityMPvpGradeGroup
            .Where(g => g.PvpGradeGroupId == season.PvpGradeGroupId)
            .OrderBy(g => g.NecessaryPvpPoint)];
        int fallbackRewardGroupId = 0;
        foreach (EntityMPvpGradeGroup grade in gradeGroups)
        {
            if (finalPoint >= grade.NecessaryPvpPoint)
            {
                fallbackRewardGroupId = grade.PvpGradeWeeklyRewardGroupId;
            }
            else
            {
                break;
            }
        }
        return fallbackRewardGroupId;
    }

    private int ResolvePvpWeeklyRankRewardGroupId(EntityMPvpSeason season, int finalRank)
        => ResolvePvpRankRewardGroupId(
            season.PvpWeeklyRankRewardRankGroupId,
            finalRank,
            _masterDb.EntityMPvpWeeklyRankRewardRankGroup,
            g => g.PvpWeeklyRankRewardRankGroupId,
            g => g.RankLowerLimit,
            g => g.PvpWeeklyRankRewardGroupId);

    private int ResolvePvpSeasonRankRewardGroupId(EntityMPvpSeason season, int finalRank)
        => ResolvePvpRankRewardGroupId(
            season.PvpSeasonRankRewardRankGroupId,
            finalRank,
            _masterDb.EntityMPvpSeasonRankRewardRankGroup,
            g => g.PvpSeasonRankRewardRankGroupId,
            g => g.RankLowerLimit,
            g => g.PvpSeasonRankRewardGroupId);

    private static int ResolvePvpRankRewardGroupId<T>(
        int rankGroupId,
        int finalRank,
        IEnumerable<T> groups,
        Func<T, int> rankGroupIdSelector,
        Func<T, int> lowerLimitSelector,
        Func<T, int> rewardGroupIdSelector)
    {
        if (rankGroupId == 0 || finalRank <= 0)
        {
            return 0;
        }

        int rewardGroupId = 0;
        foreach (T group in groups
            .Where(g => rankGroupIdSelector(g) == rankGroupId)
            .OrderBy(g => lowerLimitSelector(g)))
        {
            if (finalRank >= lowerLimitSelector(group))
            {
                rewardGroupId = rewardGroupIdSelector(group);
            }
            else
            {
                break;
            }
        }

        return rewardGroupId;
    }

    private List<EntityMPvpReward> CollectPvpRewards<T>(
        int rewardGroupId,
        IEnumerable<T> groups,
        Func<T, int> rewardGroupIdSelector,
        Func<T, int> rewardIdSelector,
        Func<T, int> sortOrderSelector)
    {
        if (rewardGroupId == 0)
        {
            return [];
        }

        List<int> rewardIds = [.. groups
            .Where(g => rewardGroupIdSelector(g) == rewardGroupId)
            .OrderBy(sortOrderSelector)
            .Select(rewardIdSelector)];
        return [.. rewardIds.Join(
            _masterDb.EntityMPvpReward,
            rewardId => rewardId,
            reward => reward.PvpRewardId,
            (_, reward) => reward)];
    }

    private void ApplyPvpRewards(DarkUserMemoryDatabase userDb, List<EntityMPvpReward> rewards, long nowMs)
    {
        if (rewards.Count == 0)
        {
            return;
        }

        // The result dialog says "Rewards were sent to your gift box", so they go there and are received from the gift box.
        foreach (EntityMPvpReward reward in rewards)
        {
            userDb.EntitySUserGift.Add(new EntitySUserGift
            {
                UserId = userDb.UserId,
                UserGiftUuid = Guid.NewGuid().ToString(),
                PossessionType = (int)reward.PossessionType,
                PossessionId = reward.PossessionId,
                Count = reward.Count,
                GrantDatetime = nowMs,
                ExpirationDatetime = nowMs + GameConfig.GiftExpirationDays * 24L * 60 * 60 * 1000
            });
        }
    }

}
