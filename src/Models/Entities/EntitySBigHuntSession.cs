namespace MariesWonderland.Models.Entities;

/// <summary>
/// Server-side BigHunt session state. Tracks in-progress battle data and deck selection.
/// Never sent to the client.
/// </summary>
public class EntitySBigHuntSession : IUserEntity
{
    public long UserId { get; set; }
    public byte[] BattleBinary { get; set; } = [];
    public int DeckNumber { get; set; }
    public int DeckType { get; set; }
    public int UserTripleDeckNumber { get; set; }
    public int BossKnockDownCount { get; set; }
    public int MaxComboCount { get; set; }
    public long TotalDamage { get; set; }
    public List<EntitySBigHuntBattleReportCostume> BattleReportCostume { get; set; } = [];
}

public class EntitySBigHuntBattleReportCostume
{
    public int WaveIndex { get; set; }
    public int CostumeId { get; set; }
    public long TotalDamage { get; set; }
    public int HitCount { get; set; }
    public int RandomDisplayValueType { get; set; }
    public long RandomDisplayValue { get; set; }
}
