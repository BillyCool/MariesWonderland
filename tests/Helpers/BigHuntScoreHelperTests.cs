using MariesWonderland.Helpers;

namespace MariesWonderland.Tests.Helpers;

/// <summary>Big Hunt survival and max-combo bonuses.</summary>
public class BigHuntScoreHelperTests
{
    [Theory]
    [InlineData(0, 2000)]
    [InlineData(1, 1500)]
    [InlineData(2, 1000)]
    [InlineData(3, 500)]
    [InlineData(4, 0)]
    [InlineData(9, 0)]
    public void AliveBonus_Is200PercentLess50PerDeath_NeverNegative(int deaths, int expectedPermil)
        => Assert.Equal(expectedPermil, BigHuntScoreHelper.AliveBonusPermil(deaths));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 0)]
    [InlineData(6, 200)]
    [InlineData(10, 200)]
    [InlineData(11, 400)]
    [InlineData(15, 400)]
    [InlineData(16, 600)]
    [InlineData(20, 600)]
    [InlineData(21, 800)]
    [InlineData(25, 800)]
    [InlineData(26, 1000)]
    [InlineData(30, 1000)]
    [InlineData(31, 1200)]
    [InlineData(35, 1200)]
    [InlineData(36, 1400)]
    [InlineData(41, 1400)]
    [InlineData(42, 1600)]
    [InlineData(45, 1600)]
    [InlineData(46, 1800)]
    [InlineData(500, 1800)]
    public void MaxComboBonus_FollowsTheTiers(int maxCombo, int expectedPermil)
        => Assert.Equal(expectedPermil, BigHuntScoreHelper.MaxComboBonusPermil(maxCombo));
}
