using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Explore;

namespace MariesWonderland.Services;

public class ExploreService(UserDataStore store, DarkMasterMemoryDatabase masterDb)
    : MariesWonderland.Proto.Explore.ExploreService.ExploreServiceBase
{
    private const int StaminaRecovery = 1000;
    private const int RewardMaterialId = 100001;
    private const int RewardBaseCount = 1;

    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Begins an explore expedition: deducts the consumable ticket and records the active expedition.</summary>
    public override Task<StartExploreResponse> StartExplore(StartExploreRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMExplore? explore = _masterDb.EntityMExplore
            .FirstOrDefault(e => e.ExploreId == request.ExploreId);

        if (explore is null)
        {
            return Task.FromResult(new StartExploreResponse());
        }

        // Deduct consumable ticket if required
        if (request.UseConsumableItemId > 0 && explore.ConsumeItemCount > 0)
        {
            EntityIUserConsumableItem? item = userDb.EntityIUserConsumableItem
                .FirstOrDefault(i => i.ConsumableItemId == request.UseConsumableItemId);

            if (item is not null)
            {
                item.Count -= explore.ConsumeItemCount;
            }
        }

        // Record or update the active expedition state
        EntityIUserExplore? userExplore = userDb.EntityIUserExplore
            .FirstOrDefault(e => e.UserId == userId);

        if (userExplore is null)
        {
            userDb.EntityIUserExplore.Add(new EntityIUserExplore
            {
                UserId = userId,
                PlayingExploreId = request.ExploreId,
                IsUseExploreTicket = false,
                LatestPlayDatetime = nowMs
            });
        }
        else
        {
            userExplore.PlayingExploreId = request.ExploreId;
            userExplore.IsUseExploreTicket = false;
            userExplore.LatestPlayDatetime = nowMs;
        }

        return Task.FromResult(new StartExploreResponse());
    }

    /// <summary>Completes an explore expedition: updates the high score, clears the active state, recovers stamina, and grants material rewards.</summary>
    public override Task<FinishExploreResponse> FinishExplore(FinishExploreRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMExplore? explore = _masterDb.EntityMExplore
            .FirstOrDefault(e => e.ExploreId == request.ExploreId);

        if (explore is null)
        {
            return Task.FromResult(new FinishExploreResponse());
        }

        int rewardCount = RewardBaseCount * explore.RewardLotteryCount;

        // Update or create score
        EntityIUserExploreScore? score = userDb.EntityIUserExploreScore
            .FirstOrDefault(s => s.ExploreId == request.ExploreId);

        if (score is null)
        {
            userDb.EntityIUserExploreScore.Add(new EntityIUserExploreScore
            {
                UserId = userId,
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
        EntityIUserExplore? userExplore = userDb.EntityIUserExplore
            .FirstOrDefault(e => e.UserId == userId);

        if (userExplore is not null)
        {
            userExplore.PlayingExploreId = 0;
            userExplore.IsUseExploreTicket = false;
        }

        // Recover stamina
        EntityIUserStatus? status = userDb.EntityIUserStatus
            .FirstOrDefault(s => s.UserId == userId);

        if (status is not null)
        {
            status.StaminaMilliValue += StaminaRecovery;
            status.StaminaUpdateDatetime = nowMs;
        }

        // Grant material reward
        EntityIUserMaterial? material = userDb.EntityIUserMaterial
            .FirstOrDefault(m => m.MaterialId == RewardMaterialId);

        if (material is not null)
        {
            material.Count += rewardCount;
        }
        else
        {
            userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
            {
                UserId = userId,
                MaterialId = RewardMaterialId,
                Count = rewardCount,
                FirstAcquisitionDatetime = nowMs
            });
        }

        // Determine grade icon
        int assetGradeIconId = GradeForScore(request.ExploreId, request.Score);

        FinishExploreResponse response = new()
        {
            AcquireStaminaCount = StaminaRecovery,
            AssetGradeIconId = assetGradeIconId
        };

        response.ExploreReward.Add(new ExploreReward
        {
            PossessionType = (int)Models.Type.PossessionType.MATERIAL,
            PossessionId = RewardMaterialId,
            Count = rewardCount
        });

        return Task.FromResult(response);
    }

    /// <summary>Cancels an in-progress explore expedition without granting rewards.</summary>
    public override Task<RetireExploreResponse> RetireExplore(RetireExploreRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserExplore? userExplore = userDb.EntityIUserExplore
            .FirstOrDefault(e => e.UserId == userId);

        if (userExplore is not null)
        {
            userExplore.PlayingExploreId = 0;
            userExplore.IsUseExploreTicket = false;
        }

        return Task.FromResult(new RetireExploreResponse());
    }

    /// <summary>Resolves the grade icon for a given explore score by checking thresholds in descending order.</summary>
    private int GradeForScore(int exploreId, int score)
    {
        // Grade scores sorted descending by NecessaryScore; first match where score >= threshold wins
        List<EntityMExploreGradeScore> gradeScores = [.. _masterDb.EntityMExploreGradeScore
            .Where(gs => gs.ExploreId == exploreId)
            .OrderByDescending(gs => gs.NecessaryScore)];

        foreach (EntityMExploreGradeScore gs in gradeScores)
        {
            if (score >= gs.NecessaryScore)
            {
                EntityMExploreGradeAsset? asset = _masterDb.EntityMExploreGradeAsset
                    .FirstOrDefault(a => a.ExploreGradeId == gs.ExploreGradeId);

                return asset?.AssetGradeIconId ?? 0;
            }
        }

        return 0;
    }
}
