using MariesWonderland.Configuration;
using MariesWonderland.Costumes;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Costume;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using CostumeServiceImpl = MariesWonderland.Services.CostumeService;

namespace MariesWonderland.Tests.Costumes;

/// <summary>
/// Costume karma: slots unlocked in order with karma stones and gold, Karma Flux applying straight to an empty
/// slot and waiting to be kept or declined on a filled one, the effect rows, and the karma missions.
/// </summary>
public class CostumeKarmaTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long UserId = 1;
    private const string Uuid = "karma-costume";
    // Released 2023-08-18; slot I rolls stat boosts (odds group 1008), slots II and III abilities.
    private const int CostumeId = 32030;
    private const int KarmaStone = 315001;

    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
    private static long NowMs => Now.ToUnixTimeMilliseconds();

    /// <summary>Always rolls the same position in a weighted list (the lowest by default, i.e. the first row).</summary>
    private sealed class FixedRandom(int value = 0) : Random
    {
        public override int Next(int maxValue) => Math.Min(value, maxValue - 1);
    }

    [Fact]
    public void RealSave_SlotResultsRebuildItsEffectRows()
    {
        DarkUserMemoryDatabase save = new UserDataSeeder(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = "Data/UserData" } })).LoadFromFiles();
        Dictionary<string, int> costumeIds = save.EntityIUserCostume.ToDictionary(c => c.UserCostumeUuid, c => c.CostumeId);
        Assert.All(save.EntityIUserCostumeLotteryEffect.Where(r => r.OddsNumber > 0),
            r => Assert.NotNull(CostumeKarmaHelper.FindResult(MasterDb, costumeIds[r.UserCostumeUuid], r.SlotNumber, r.OddsNumber)));

        List<string> abilities = [.. save.EntityIUserCostumeLotteryEffectAbility.Select(r => $"{r.UserCostumeUuid}/{r.SlotNumber}/{r.AbilityId}/{r.AbilityLevel}").Order()];
        List<string> stats = [.. save.EntityIUserCostumeLotteryEffectStatusUp.Select(StatKey).Order()];
        save.EntityIUserCostumeLotteryEffectAbility.Clear();
        save.EntityIUserCostumeLotteryEffectStatusUp.Clear();

        foreach (string uuid in save.EntityIUserCostumeLotteryEffect.Select(r => r.UserCostumeUuid).Distinct())
        {
            CostumeKarmaHelper.RefreshEffects(save, MasterDb, uuid);
        }

        Assert.Equal(abilities, [.. save.EntityIUserCostumeLotteryEffectAbility.Select(r => $"{r.UserCostumeUuid}/{r.SlotNumber}/{r.AbilityId}/{r.AbilityLevel}").Order()]);
        Assert.Equal(stats, [.. save.EntityIUserCostumeLotteryEffectStatusUp.Select(StatKey).Order()]);
    }

    [Fact]
    public void UnlockSlot_GoesInOrderAndCostsStonesAndGold()
    {
        DarkUserMemoryDatabase userDb = CreateKarmaUser(stones: 30, gold: 1_000_000);

        Assert.False(Unlock(userDb, 2));
        Assert.True(Unlock(userDb, 1));
        Assert.False(Unlock(userDb, 1));
        Assert.True(Unlock(userDb, 2));

        Assert.Equal([1, 2], userDb.EntityIUserCostumeLotteryEffect.Select(r => r.SlotNumber).Order());
        Assert.All(userDb.EntityIUserCostumeLotteryEffect, r => Assert.Equal(0, r.OddsNumber));
        Assert.Equal(10, Material(userDb, KarmaStone));
        Assert.Equal(1_000_000 - 2 * GameConfig.CostumeLotteryEffectUnlockSlotConsumeGold, Gold(userDb));
    }

    [Fact]
    public void UnlockSlot_ShortOfStonesOrGold_ChangesNothing()
    {
        DarkUserMemoryDatabase stoneShort = CreateKarmaUser(stones: 9, gold: 1_000_000);
        DarkUserMemoryDatabase goldShort = CreateKarmaUser(stones: 10, gold: GameConfig.CostumeLotteryEffectUnlockSlotConsumeGold - 1);

        Assert.False(Unlock(stoneShort, 1));
        Assert.False(Unlock(goldShort, 1));

        Assert.Empty(stoneShort.EntityIUserCostumeLotteryEffect);
        Assert.Equal(9, Material(stoneShort, KarmaStone));
        Assert.Equal(1_000_000, Gold(stoneShort));
        Assert.Empty(goldShort.EntityIUserCostumeLotteryEffect);
        Assert.Equal(10, Material(goldShort, KarmaStone));
    }

    [Fact]
    public void Draw_OnAnEmptySlot_AppliesTheResultAndPaysTheDrawCost()
    {
        DarkUserMemoryDatabase userDb = CreateKarmaUser(stones: 30, gold: 1_000_000, draws: 2);
        Assert.True(Unlock(userDb, 1));
        Assert.True(Unlock(userDb, 2));
        int goldBefore = Gold(userDb);

        Assert.True(Draw(userDb, 1, new FixedRandom()));
        Assert.True(Draw(userDb, 2, new FixedRandom()));

        // The first row of each odds group: slot I "Agility +150, HP +100%", slot II its group's first ability at level 3.
        Assert.Equal(1, Slot(userDb, 1).OddsNumber);
        Assert.Equal(1, Slot(userDb, 2).OddsNumber);
        Assert.Empty(userDb.EntityIUserCostumeLotteryEffectPending);

        EntityMCostumeLotteryEffectTargetAbility ability = MasterDb.EntityMCostumeLotteryEffectTargetAbility.Find(
            CostumeKarmaHelper.FindResult(MasterDb, CostumeId, 2, 1)!.CostumeLotteryEffectTargetId)!;
        EntityIUserCostumeLotteryEffectAbility abilityRow = Assert.Single(userDb.EntityIUserCostumeLotteryEffectAbility);
        Assert.Equal((2, ability.AbilityId, 3), (abilityRow.SlotNumber, abilityRow.AbilityId, abilityRow.AbilityLevel));

        EntityIUserCostumeLotteryEffectStatusUp flat = StatRow(userDb, StatusCalculationType.ADD);
        EntityIUserCostumeLotteryEffectStatusUp percent = StatRow(userDb, StatusCalculationType.MULTIPLY);
        Assert.Equal(150, flat.Agility);
        Assert.Equal(100, percent.Hp);
        Assert.Equal(2, userDb.EntityIUserCostumeLotteryEffectStatusUp.Count);

        Assert.Equal(goldBefore - 2 * GameConfig.CostumeLotteryEffectDrawSlotConsumeGold, Gold(userDb));
        Assert.All(DrawCosts(), cost => Assert.Equal(0, Material(userDb, cost.MaterialId)));
    }

    [Fact]
    public void Draw_OnAFilledSlot_WaitsToBeKeptOrDeclined()
    {
        DarkUserMemoryDatabase userDb = CreateKarmaUser(stones: 10, gold: 1_000_000, draws: 3);
        Assert.True(Unlock(userDb, 1));
        Assert.True(Draw(userDb, 1, new FixedRandom()));

        // A second flux leaves slot I as it is and holds the result (the last row, "HP +100%") for the player.
        Assert.True(Draw(userDb, 1, new FixedRandom(int.MaxValue)));
        Assert.Equal(1, Slot(userDb, 1).OddsNumber);
        EntityIUserCostumeLotteryEffectPending pending = Assert.Single(userDb.EntityIUserCostumeLotteryEffectPending);
        Assert.Equal((1, 15), (pending.SlotNumber, pending.OddsNumber));

        // Declined: the old effect stays.
        Assert.True(CostumeKarmaHelper.Confirm(userDb, MasterDb, Uuid, isAccept: false));
        Assert.Empty(userDb.EntityIUserCostumeLotteryEffectPending);
        Assert.Equal(1, Slot(userDb, 1).OddsNumber);
        Assert.Equal(150, StatRow(userDb, StatusCalculationType.ADD).Agility);

        // Kept: the slot takes it and the flat agility row goes, since no slot gives a flat stat any more.
        Assert.True(Draw(userDb, 1, new FixedRandom(int.MaxValue)));
        Assert.True(CostumeKarmaHelper.Confirm(userDb, MasterDb, Uuid, isAccept: true));
        Assert.Equal(15, Slot(userDb, 1).OddsNumber);
        Assert.Empty(userDb.EntityIUserCostumeLotteryEffectPending);
        EntityIUserCostumeLotteryEffectStatusUp only = Assert.Single(userDb.EntityIUserCostumeLotteryEffectStatusUp);
        Assert.Equal((StatusCalculationType.MULTIPLY, 100, 0), (only.StatusCalculationType, only.Hp, only.Agility));

        Assert.False(CostumeKarmaHelper.Confirm(userDb, MasterDb, Uuid, isAccept: true));
    }

    [Fact]
    public void Draw_OnALockedSlotOrShortOfMaterials_DoesNothing()
    {
        DarkUserMemoryDatabase userDb = CreateKarmaUser(stones: 10, gold: 1_000_000, draws: 0);

        Assert.False(Draw(userDb, 1, new FixedRandom()));
        Assert.True(Unlock(userDb, 1));
        int goldBefore = Gold(userDb);
        Assert.False(Draw(userDb, 1, new FixedRandom()));

        Assert.Equal(0, Slot(userDb, 1).OddsNumber);
        Assert.Equal(goldBefore, Gold(userDb));
        Assert.Empty(userDb.EntityIUserCostumeLotteryEffectStatusUp);
    }

    [Fact]
    public async Task Rpcs_CountTheKarmaMissions()
    {
        // "Karma 1 time" and "Karma Flux 1 time" belong to an event group with a term; run inside it.
        EntityMMission unlockMission = MasterDb.EntityMMission.Single(m => m.MissionId == 37201);
        EntityMMissionTerm term = MasterDb.EntityMMissionTerm.Find(unlockMission.MissionTermId)!;
        FakeTimeProvider time = new(DateTimeOffset.FromUnixTimeMilliseconds(term.StartDatetime + 1000));

        DarkUserMemoryDatabase userDb = CreateKarmaUser(stones: 10, gold: 1_000_000, draws: 1);
        MissionTracker missions = new(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Load(MissionFilterTable.DefaultPath)));
        missions.Refresh(userDb, time.GetUtcNow().ToUnixTimeMilliseconds());
        CostumeServiceImpl service = new(MasterDb, CreateStore(UserId, userDb), GameConfig, new EnhanceCampaignHelper(MasterDb), missions, time);

        await service.UnlockLotteryEffectSlot(new UnlockLotteryEffectSlotRequest { UserCostumeUuid = Uuid, SlotNumber = 1 }, ContextFor(UserId));
        await service.DrawLotteryEffect(new DrawLotteryEffectRequest { UserCostumeUuid = Uuid, SlotNumber = 1 }, ContextFor(UserId));
        // Nothing left to pay with: neither counts again.
        await service.UnlockLotteryEffectSlot(new UnlockLotteryEffectSlotRequest { UserCostumeUuid = Uuid, SlotNumber = 2 }, ContextFor(UserId));
        await service.DrawLotteryEffect(new DrawLotteryEffectRequest { UserCostumeUuid = Uuid, SlotNumber = 1 }, ContextFor(UserId));

        Assert.NotEqual(0, Slot(userDb, 1).OddsNumber);
        Assert.Equal(1, userDb.EntityIUserMission.Single(r => r.MissionId == 37201).ProgressValue);
        Assert.Equal(1, userDb.EntityIUserMission.Single(r => r.MissionId == 37204).ProgressValue);
    }

    private bool Unlock(DarkUserMemoryDatabase userDb, int slotNumber)
        => CostumeKarmaHelper.UnlockSlot(userDb, MasterDb, GameConfig, Uuid, slotNumber, NowMs);

    private bool Draw(DarkUserMemoryDatabase userDb, int slotNumber, Random random)
        => CostumeKarmaHelper.Draw(userDb, MasterDb, GameConfig, Uuid, slotNumber, random, NowMs);

    private List<EntityMCostumeLotteryEffectMaterialGroup> DrawCosts()
    {
        int groupId = MasterDb.EntityMCostumeLotteryEffect.Find(CostumeId, 1)!.CostumeLotteryEffectDrawMaterialGroupId;
        return [.. MasterDb.EntityMCostumeLotteryEffectMaterialGroup.Where(g => g.CostumeLotteryEffectMaterialGroupId == groupId)];
    }

    /// <summary>A user owning the karma costume, <paramref name="stones"/> karma stones, gold, and the materials for <paramref name="draws"/> Karma Flux.</summary>
    private DarkUserMemoryDatabase CreateKarmaUser(int stones, int gold, int draws = 0)
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.UserId = UserId;
        userDb.EntityIUserCostume.Add(new EntityIUserCostume { UserId = UserId, UserCostumeUuid = Uuid, CostumeId = CostumeId, Level = 1 });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = KarmaStone, Count = stones });
        foreach (EntityMCostumeLotteryEffectMaterialGroup cost in DrawCosts())
        {
            userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = cost.MaterialId, Count = cost.Count * draws });
        }
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem { UserId = UserId, ConsumableItemId = GameConfig.ConsumableItemIdForGold, Count = gold });
        return userDb;
    }

    private static EntityIUserCostumeLotteryEffect Slot(DarkUserMemoryDatabase userDb, int slotNumber)
        => userDb.EntityIUserCostumeLotteryEffect.Single(r => r.SlotNumber == slotNumber);

    private static EntityIUserCostumeLotteryEffectStatusUp StatRow(DarkUserMemoryDatabase userDb, StatusCalculationType calculationType)
        => userDb.EntityIUserCostumeLotteryEffectStatusUp.Single(r => r.StatusCalculationType == calculationType);

    private static int Material(DarkUserMemoryDatabase userDb, int materialId)
        => userDb.EntityIUserMaterial.Single(m => m.MaterialId == materialId).Count;

    private int Gold(DarkUserMemoryDatabase userDb)
        => userDb.EntityIUserConsumableItem.Single(c => c.ConsumableItemId == GameConfig.ConsumableItemIdForGold).Count;

    private static string StatKey(EntityIUserCostumeLotteryEffectStatusUp r)
        => $"{r.UserCostumeUuid}/{r.StatusCalculationType}/{r.Hp}/{r.Attack}/{r.Vitality}/{r.Agility}/{r.CriticalRatio}/{r.CriticalAttack}";
}
