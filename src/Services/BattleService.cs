using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Battle;

namespace MariesWonderland.Services;

public class BattleService(UserDataStore store, DarkMasterMemoryDatabase masterDb, MissionTracker missions, TimeProvider timeProvider) : MariesWonderland.Proto.Battle.BattleService.BattleServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Initializes a battle wave; currently a no-op that returns empty response.</summary>
    public override Task<StartWaveResponse> StartWave(StartWaveRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        return Task.FromResult(new StartWaveResponse());
    }

    /// <summary>Records battle results, tracks shortest clear time and counts the bosses defeated in the wave.</summary>
    public override Task<FinishWaveResponse> FinishWave(FinishWaveRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        BattleDetail? detail = request.BattleDetail;

        // Find the currently active quest so we can associate this battle with it: the one started last, as an
        // abandoned run leaves its quest active.
        EntityIUserQuest? activeQuest = userDb.EntityIUserQuest
            .Where(q => q.QuestStateType == (int)QuestStateType.ACTIVE)
            .OrderByDescending(q => q.LatestStartDatetime)
            .FirstOrDefault();

        // Save full battle record mirroring the proto request structure.
        EntitySBattleDetail battleRecord = new()
        {
            QuestId = activeQuest?.QuestId ?? 0,
            UserId = userId,
            ElapsedFrameCount = request.ElapsedFrameCount,
            BattleBinary = request.BattleBinary.ToBase64(),
            RecordedAt = _timeProvider.GetUtcNow().UtcDateTime,
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
        _store.RecordBattle(battleRecord);

        if (activeQuest != null)
        {
            // Track shortest clear time.
            if (activeQuest.ShortestClearFrames == 0 || request.ElapsedFrameCount < activeQuest.ShortestClearFrames)
            {
                activeQuest.ShortestClearFrames = (int)request.ElapsedFrameCount;
            }
        }

        int defeatedBosses = request.NpcPartyResultInfoList.Sum(DefeatedBosses);
        if (defeatedBosses > 0)
        {
            _missions.Record(userDb, MissionClearConditionType.DEFEAT_BOSS_COUNT, defeatedBosses, nowMs);
        }

        return Task.FromResult(new FinishWaveResponse());
    }

    /// <summary>
    /// The bosses of an enemy party the wave wiped out: its deck's members typed BOSS in master data, counted only
    /// when every member's HP was depleted. The result reports how many were, not which ones.
    /// </summary>
    private int DefeatedBosses(NpcPartyResultInfo npcParty)
    {
        EntityMBattleNpcDeck? deck = _masterDb.EntityMBattleNpcDeck.Find(npcParty.NpcId, (DeckType)npcParty.DeckType, npcParty.BattleNpcDeckNumber);
        if (deck == null)
        {
            return 0;
        }

        List<string> members = [.. new[] { deck.BattleNpcDeckCharacterUuid01, deck.BattleNpcDeckCharacterUuid02, deck.BattleNpcDeckCharacterUuid03 }
            .Where(uuid => !string.IsNullOrEmpty(uuid))];
        if (members.Count == 0 || npcParty.CharacterHpDepletedCount < members.Count)
        {
            return 0;
        }

        return members.Count(uuid => _masterDb.EntityMBattleNpcDeckCharacterType.Find(deck.BattleNpcId, uuid)?.BattleEnemyType == BattleEnemyType.BOSS);
    }
}
