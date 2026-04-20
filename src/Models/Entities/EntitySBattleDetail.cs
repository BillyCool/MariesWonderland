using MariesWonderland.Proto.Battle;

namespace MariesWonderland.Models.Entities;

/// <summary>
/// Server-side battle record captured at FinishWave. Never sent to the client.
/// </summary>
public class EntitySBattleDetail : IUserEntity
{
    // Server-side context fields (not in proto).
    public long UserId { get; set; }
    public int QuestId { get; set; }
    public DateTime RecordedAt { get; set; }

    // Top-level FinishWaveRequest fields.
    /// <summary>Raw base64-encoded MessagePack blob from FinishWaveRequest.battleBinary.</summary>
    public string BattleBinary { get; set; } = string.Empty;
    public long ElapsedFrameCount { get; set; }

    // BattleDetail message scalar fields.
    public int MaxDamage { get; set; }
    public int PlayerCostumeActiveSkillUsedCount { get; set; }
    public int PlayerWeaponActiveSkillUsedCount { get; set; }
    public int PlayerCompanionSkillUsedCount { get; set; }
    public int CriticalCount { get; set; }
    public int ComboCount { get; set; }
    public int ComboMaxDamage { get; set; }
    public long TotalRecoverPoint { get; set; }

    /// <summary>Computed: count of costumes in CostumeBattleInfoList where IsAlive is false.</summary>
    public int CharacterDeathCount => CostumeBattleInfoList.Count(c => !c.IsAlive);

    // Nested lists — use proto-generated types directly.
    public List<CostumeBattleInfo> CostumeBattleInfoList { get; set; } = [];
    public List<UserPartyResultInfo> UserPartyResultInfoList { get; set; } = [];
    public List<NpcPartyResultInfo> NpcPartyResultInfoList { get; set; } = [];
}
