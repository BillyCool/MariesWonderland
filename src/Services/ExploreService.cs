using Grpc.Core;
using MariesWonderland.Constants;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Explore;

namespace MariesWonderland.Services;

public class ExploreService(UserDataStore store, DarkMasterMemoryDatabase masterDb, GameConfig gameConfig)
    : MariesWonderland.Proto.Explore.ExploreService.ExploreServiceBase
{
    // EXP granted per run = 0.5% of the user's current level EXP requirement × RewardLotteryCount.
    // Normal explore (×1) grants 0.5%; Hard (×11) grants 5.5%.
    private const double ExploreExpRewardBasePercent = 0.005;

    // Stamina units (not milli); total = base × RewardLotteryCount
    private const int ExploreStaminaRewardBase = 50;

    // Each lottery draw grants this many of the pulled material

    // TODO: Review and add more material IDs to expand the loot pool
    private static readonly int[] RewardMaterialPool = [100001];

    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>Begins an explore expedition: deducts the consumable ticket and records the active expedition.</summary>
    public override Task<StartExploreResponse> StartExplore(StartExploreRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMExplore explore = _masterDb.EntityMExplore
            .FirstOrDefault(e => e.ExploreId == request.ExploreId)
            ?? throw new RpcException(new Status(StatusCode.InvalidArgument, ExploreErrors.InvalidExploreId));

        if (nowMs < explore.StartDatetime)
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.ExploreNotStarted));

        EntityMExploreUnlockCondition? unlockCondition = _masterDb.EntityMExploreUnlockCondition
            .FirstOrDefault(c => c.ExploreUnlockConditionId == explore.ExploreUnlockConditionId);

        if (unlockCondition is not null && !UnlockConditionHelper.IsExploreUnlocked(request.ExploreId, unlockCondition, userDb, _masterDb))
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.UnlockConditionNotMet));

        EntityIUserExplore userExplore = userDb.EntityIUserExplore.FirstOrDefault()
            ?? throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.UserExploreNotFound));

        bool isUsingTicket = request.UseConsumableItemId == _gameConfig.ConsumableItemIdForExploreTicket;

        // Deduct explore ticket if the request specifies it
        if (isUsingTicket && explore.ConsumeItemCount > 0)
        {
            EntityIUserConsumableItem item = userDb.EntityIUserConsumableItem
                .FirstOrDefault(i => i.ConsumableItemId == request.UseConsumableItemId)
                ?? throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.TicketNotFound));

            if (item.Count < explore.ConsumeItemCount)
                throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.InsufficientTickets));

            item.Count -= explore.ConsumeItemCount;
        }

        // Record or update the active expedition state
        userExplore.PlayingExploreId = request.ExploreId;
        userExplore.IsUseExploreTicket = isUsingTicket;
        userExplore.LatestPlayDatetime = nowMs;

        return Task.FromResult(new StartExploreResponse());
    }

    /// <summary>Completes an explore expedition: updates the high score, clears the active state, recovers stamina, and grants material rewards.</summary>
    public override Task<FinishExploreResponse> FinishExplore(FinishExploreRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMExplore explore = _masterDb.EntityMExplore
            .FirstOrDefault(e => e.ExploreId == request.ExploreId)
            ?? throw new RpcException(new Status(StatusCode.InvalidArgument, ExploreErrors.InvalidExploreId));

        EntityIUserExplore userExplore = userDb.EntityIUserExplore.FirstOrDefault()
            ?? throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.UserExploreNotFound));

        if (userExplore.PlayingExploreId != request.ExploreId)
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.NoMatchingActiveExploreToFinish));

        EntityIUserStatus status = userDb.EntityIUserStatus.FirstOrDefault()
            ?? throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.UserStatusNotFound));

        int staminaReward = ExploreStaminaRewardBase * explore.RewardLotteryCount;
        int expReward = ComputeExpReward(status.Level, explore.RewardLotteryCount);

        // Update or create score record — only track personal best
        EntityIUserExploreScore? score = userDb.EntityIUserExploreScore
            .FirstOrDefault(s => s.ExploreId == request.ExploreId);

        if (score is null)
        {
            userDb.EntityIUserExploreScore.Add(new EntityIUserExploreScore
            {
                UserId = userDb.UserId,
                ExploreId = request.ExploreId,
                MaxScore = request.Score,
                MaxScoreUpdateDatetime = nowMs
            });
        }
        else if (request.Score > score.MaxScore)
        {
            score.MaxScore = request.Score;
            score.MaxScoreUpdateDatetime = nowMs;
        }

        // Clear playing state
        userExplore.PlayingExploreId = 0;
        userExplore.IsUseExploreTicket = false;

        // Recover stamina (capped at the configured maximum)
        StaminaHelper.AddStamina(status, staminaReward, _gameConfig, nowMs);

        // Grant EXP; max gain per run is 5.5% of a level so at most one level-up is possible
        status.Exp += expReward;
        int mapId = _gameConfig.UserLevelExpNumericalParameterMapId;
        EntityMNumericalParameterMap? nextLevelEntry = _masterDb.EntityMNumericalParameterMap
            .FirstOrDefault(x => x.NumericalParameterMapId == mapId && x.ParameterKey == status.Level + 1);
        if (nextLevelEntry is not null && status.Exp >= nextLevelEntry.ParameterValue)
            status.Level++;

        // Resolve grade and compute per-draw stack size
        int gradeId = ResolveGradeId(request.ExploreId, request.Score);
        int assetGradeIconId = _masterDb.EntityMExploreGradeAsset
            .FirstOrDefault(a => a.ExploreGradeId == gradeId)?.AssetGradeIconId ?? 0;
        int stackPerDraw = GradeRewardStack(gradeId);

        FinishExploreResponse response = new()
        {
            AcquireStaminaCount = staminaReward,
            AssetGradeIconId = assetGradeIconId
        };

        // Grade 106 (worst) yields no material; skip drawing entirely to avoid zero-count reward entries
        if (stackPerDraw > 0)
        {
            // Draw RewardLotteryCount items from the pool (with replacement); stack size per draw is grade-dependent
            Dictionary<int, int> draws = [];
            for (int i = 0; i < explore.RewardLotteryCount; i++)
            {
                int materialId = RewardMaterialPool[Random.Shared.Next(RewardMaterialPool.Length)];
                draws[materialId] = draws.GetValueOrDefault(materialId) + 1;
            }

            foreach (var (materialId, drawCount) in draws)
            {
                int totalCount = drawCount * stackPerDraw;
                PossessionHelper.Apply(userDb, PossessionType.MATERIAL, materialId, totalCount, _masterDb);
                response.ExploreReward.Add(new ExploreReward
                {
                    PossessionType = (int)PossessionType.MATERIAL,
                    PossessionId = materialId,
                    Count = totalCount
                });
            }
        }

        return Task.FromResult(response);
    }

    /// <summary>Cancels an in-progress explore expedition without granting rewards.</summary>
    public override Task<RetireExploreResponse> RetireExplore(RetireExploreRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());

        EntityIUserExplore userExplore = userDb.EntityIUserExplore.FirstOrDefault()
            ?? throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.UserExploreNotFound));

        if (userExplore.PlayingExploreId != request.ExploreId)
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ExploreErrors.NoMatchingActiveExploreToRetire));

        userExplore.PlayingExploreId = 0;
        userExplore.IsUseExploreTicket = false;

        return Task.FromResult(new RetireExploreResponse());
    }

    /// <summary>
    /// Computes EXP to grant for an explore run: 0.5% of the EXP required to complete
    /// the user's current level, multiplied by <paramref name="lotteryCount"/>.
    /// Returns 0 if the level thresholds are not available in master data.
    /// </summary>
    private int ComputeExpReward(int level, int lotteryCount)
    {
        int mapId = _gameConfig.UserLevelExpNumericalParameterMapId;

        EntityMNumericalParameterMap? current = _masterDb.EntityMNumericalParameterMap
            .FirstOrDefault(x => x.NumericalParameterMapId == mapId && x.ParameterKey == level);
        EntityMNumericalParameterMap? next = _masterDb.EntityMNumericalParameterMap
            .FirstOrDefault(x => x.NumericalParameterMapId == mapId && x.ParameterKey == level + 1);

        if (current is null || next is null)
            return 0;

        int levelRequirement = next.ParameterValue - current.ParameterValue;
        return (int)(levelRequirement * ExploreExpRewardBasePercent * lotteryCount);
    }

    /// <summary>Resolves the ExploreGradeId for a given score by checking thresholds in descending order.</summary>
    private int ResolveGradeId(int exploreId, int score)
    {
        IEnumerable<EntityMExploreGradeScore> gradeScores = _masterDb.EntityMExploreGradeScore
            .Where(gs => gs.ExploreId == exploreId)
            .OrderByDescending(gs => gs.NecessaryScore);

        foreach (EntityMExploreGradeScore gs in gradeScores)
        {
            if (score >= gs.NecessaryScore)
                return gs.ExploreGradeId;
        }

        return 0;
    }

    /// <summary>
    /// Returns the number of material items to grant per lottery draw based on the grade.
    /// Grades 101 (best) through 106 (worst). Grade 106 yields 0 — no material rewards.
    /// Values are design estimates — no master data source.
    /// </summary>
    private static int GradeRewardStack(int gradeId) => gradeId switch
    {
        101 => 10,
        102 => 8,
        103 => 6,
        104 => 4,
        105 => 2,
        _ => 0  // grade 106 (worst) or unknown — no material rewards
    };
}
