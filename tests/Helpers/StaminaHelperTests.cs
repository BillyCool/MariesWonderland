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
}
