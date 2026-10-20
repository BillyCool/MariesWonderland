using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;

namespace MariesWonderland.Tests.Helpers;

public class StaminaHelperTests
{
    private static GameConfig ConfigWithCap(int cap) => new() { StaminaMaxCount = cap };

    [Fact]
    public void AddStamina_BelowCap_AddsFullAmount()
    {
        var status = new EntityIUserStatus { StaminaMilliValue = 0 };

        StaminaHelper.AddStamina(status, 50, ConfigWithCap(999), nowMs: 1000L);

        Assert.Equal(50_000, status.StaminaMilliValue);
        Assert.Equal(1000L, status.StaminaUpdateDatetime);
    }

    [Fact]
    public void AddStamina_ExceedsCap_ClampsToMax()
    {
        // Start near cap (990 stamina), add 50 → should clamp to 999
        var status = new EntityIUserStatus { StaminaMilliValue = 990_000 };

        StaminaHelper.AddStamina(status, 50, ConfigWithCap(999), nowMs: 2000L);

        Assert.Equal(999_000, status.StaminaMilliValue);
    }

    [Fact]
    public void AddStamina_AlreadyAtCap_DoesNotExceed()
    {
        var status = new EntityIUserStatus { StaminaMilliValue = 999_000 };

        StaminaHelper.AddStamina(status, 50, ConfigWithCap(999), nowMs: 3000L);

        Assert.Equal(999_000, status.StaminaMilliValue);
    }

    [Fact]
    public void Consume_SettlesRegenerationBeforeSpending()
    {
        // 10 points at t=0; 30 minutes at 180 s per point regenerates 10 points.
        EntityIUserStatus status = new() { StaminaMilliValue = 10_000, StaminaUpdateDatetime = 0 };

        StaminaHelper.Consume(status, cost: 5, maxStamina: 50, recoverySecond: 180, nowMs: 30 * 60 * 1000L);

        Assert.Equal(15_000, status.StaminaMilliValue);
        Assert.Equal(30 * 60 * 1000L, status.StaminaUpdateDatetime);
    }

    [Fact]
    public void Settle_StopsAtTheMax()
    {
        EntityIUserStatus status = new() { StaminaMilliValue = 49_000, StaminaUpdateDatetime = 0 };

        StaminaHelper.Settle(status, maxStamina: 50, recoverySecond: 180, nowMs: 24 * 60 * 60 * 1000L);

        Assert.Equal(50_000, status.StaminaMilliValue);
    }

    [Fact]
    public void Consume_AboveTheMax_SpendsWithoutRegenerating()
    {
        EntityIUserStatus status = new() { StaminaMilliValue = 80_000, StaminaUpdateDatetime = 0 };

        StaminaHelper.Consume(status, cost: 10, maxStamina: 50, recoverySecond: 180, nowMs: 60 * 60 * 1000L);

        Assert.Equal(70_000, status.StaminaMilliValue);
    }

    [Fact]
    public void Consume_MoreThanAvailable_StopsAtZero()
    {
        EntityIUserStatus status = new() { StaminaMilliValue = 3_000, StaminaUpdateDatetime = 0 };

        StaminaHelper.Consume(status, cost: 10, maxStamina: 50, recoverySecond: 180, nowMs: 0);

        Assert.Equal(0, status.StaminaMilliValue);
    }
}
