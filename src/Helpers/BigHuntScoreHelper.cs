namespace MariesWonderland.Helpers;

/// <summary>
/// Big Hunt score bonuses. Neither the client nor master data has these values (the client only shows what the
/// server sends); they come from the owner's memory of the real game, and the help page confirms what each depends on.
/// </summary>
public static class BigHuntScoreHelper
{
    /// <summary>Survival bonus with nobody down (permil).</summary>
    public const int MaxAliveBonusPermil = 2000;

    /// <summary>What each character who died takes off the survival bonus (permil).</summary>
    public const int AliveBonusLossPerDeathPermil = 500;

    // Lowest max combo of each tier, highest first, with its bonus (permil).
    private static readonly (int MinCombo, int BonusPermil)[] ComboTiers =
    [
        (46, 1800),
        (42, 1600),
        (36, 1400),
        (31, 1200),
        (26, 1000),
        (21, 800),
        (16, 600),
        (11, 400),
        (6, 200)
    ];

    /// <summary>
    /// Survival bonus: +200% when every character is alive at the end of their wave, 50% less for each who isn't,
    /// never below 0. Our reading of "the more characters survive, the larger the bonus"; the exact steps are recalled,
    /// not confirmed.
    /// </summary>
    public static int AliveBonusPermil(int deathCount)
        => Math.Max(0, MaxAliveBonusPermil - AliveBonusLossPerDeathPermil * Math.Max(0, deathCount));

    /// <summary>Max-combo bonus from the highest combo of the run: +20% per 5 combo from 6, +180% from 46.</summary>
    public static int MaxComboBonusPermil(int maxComboCount)
    {
        foreach ((int minCombo, int bonusPermil) in ComboTiers)
        {
            if (maxComboCount >= minCombo)
            {
                return bonusPermil;
            }
        }

        return 0;
    }
}
