using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Battle;

namespace MariesWonderland.Services;

public class BattleService(UserDataStore store) : MariesWonderland.Proto.Battle.BattleService.BattleServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Initializes a battle wave; currently a no-op that returns empty response.</summary>
    public override Task<StartWaveResponse> StartWave(StartWaveRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        return Task.FromResult(new StartWaveResponse());
    }

    /// <summary>Records battle results and tracks shortest clear time.</summary>
    public override Task<FinishWaveResponse> FinishWave(FinishWaveRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        BattleDetail? detail = request.BattleDetail;

        // Find the currently active quest so we can associate this battle with it.
        EntityIUserQuest? activeQuest = userDb.EntityIUserQuest
            .FirstOrDefault(q => q.QuestStateType == (int)QuestStateType.ACTIVE);

        // Save full battle record mirroring the proto request structure.
        EntitySBattleDetail battleRecord = new()
        {
            QuestId = activeQuest?.QuestId ?? 0,
            UserId = userId,
            ElapsedFrameCount = request.ElapsedFrameCount,
            BattleBinary = request.BattleBinary.ToBase64(),
            RecordedAt = DateTime.UtcNow,
            MaxDamage = detail?.MaxDamage ?? 0,
            PlayerCostumeActiveSkillUsedCount = detail?.PlayerCostumeActiveSkillUsedCount ?? 0,
            PlayerWeaponActiveSkillUsedCount = detail?.PlayerWeaponActiveSkillUsedCount ?? 0,
            PlayerCompanionSkillUsedCount = detail?.PlayerCompanionSkillUsedCount ?? 0,
            CriticalCount = detail?.CriticalCount ?? 0,
            ComboCount = detail?.ComboCount ?? 0,
            ComboMaxDamage = detail?.ComboMaxDamage ?? 0,
            TotalRecoverPoint = detail?.TotalRecoverPoint ?? 0,
            CostumeBattleInfoList = detail != null ? [.. detail.CostumeBattleInfo] : [],
            UserPartyResultInfoList = [.. request.UserPartyResultInfoList],
            NpcPartyResultInfoList = [.. request.NpcPartyResultInfoList],
        };
        userDb.BattleDetails.Add(battleRecord);

        if (activeQuest != null)
        {
            // Track shortest clear time.
            if (activeQuest.ShortestClearFrames == 0 || request.ElapsedFrameCount < activeQuest.ShortestClearFrames)
            {
                activeQuest.ShortestClearFrames = (int)request.ElapsedFrameCount;
            }
        }

        return Task.FromResult(new FinishWaveResponse());
    }
}
