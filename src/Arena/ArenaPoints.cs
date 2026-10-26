namespace MariesWonderland.Arena;

/// <summary>
/// How Arena points move. A win gains more against someone above you and less against someone below; a loss costs
/// less against someone stronger. The defender moves by the same amount the other way. Amounts are multiples of 12,
/// like every weekly total in the real save. Points carry over between weeks and start again each season.
/// </summary>
public static class ArenaPoints
{
    /// <summary>Points for a win against an even opponent; each 20 points the defender is above adds 1.</summary>
    public const int BaseWin = 120;

    /// <summary>Points lost to an even opponent; each 40 points the defender is above takes 1 off.</summary>
    public const int BaseLoss = 60;

    private const int Step = 12;

    public static int WinGain(int attackerPoints, int defenderPoints)
        => RoundToStep(Math.Clamp(BaseWin + (defenderPoints - attackerPoints) / 20, Step, BaseWin * 3));

    public static int LossCost(int attackerPoints, int defenderPoints)
        => RoundToStep(Math.Clamp(BaseLoss - (defenderPoints - attackerPoints) / 40, Step, BaseLoss * 3));

    /// <summary>
    /// A player's points on their first visit in a season, by their highest deck force (the real game's brackets):
    /// under 120k 18,000; under 220k 22,000; under 320k 29,000; above that 36,000.
    /// </summary>
    public static int InitialPoints(int maxDeckPower) => maxDeckPower switch
    {
        < 120_000 => 18_000,
        < 220_000 => 22_000,
        < 320_000 => 29_000,
        _ => 36_000,
    };

    private static int RoundToStep(int value) => (value + Step / 2) / Step * Step;
}
