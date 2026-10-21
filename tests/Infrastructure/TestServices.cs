using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Quests;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>Builds the shared engine pieces services take, from whichever master data a test uses.</summary>
public static class TestServices
{
    /// <summary>A mission tracker with no filter table (filtered missions never progress).</summary>
    public static MissionTracker Missions(DarkMasterMemoryDatabase masterDb)
        => new(masterDb, new MissionCatalog(masterDb, MissionFilterTable.Empty));

    /// <summary>A quest engine over <paramref name="masterDb"/>; stamina is free unless <paramref name="freeStamina"/> is false.</summary>
    public static QuestEngine QuestEngine(DarkMasterMemoryDatabase masterDb, MissionTracker? missions = null, bool freeStamina = true)
        => new(masterDb, GameConfig.From(masterDb.EntityMConfig), new QuestCampaignHelper(masterDb), new ImportantItemEffectHelper(masterDb),
            missions ?? Missions(masterDb), Options.Create(new DevModeOptions { FreeStamina = freeStamina }));
}
