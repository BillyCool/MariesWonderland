using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.CharacterBoard;
using MariesWonderland.Tests.Infrastructure;
using CharacterBoardService = MariesWonderland.Services.CharacterBoardService;

namespace MariesWonderland.Tests.Services;

public class CharacterBoardServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long UserId = 1L;

    private static CharacterBoardService CreateService(DarkMasterMemoryDatabase masterDb, UserDataStore store) => new(masterDb, store);

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static void AddPanel(
        DarkMasterMemoryDatabase masterDb,
        int panelId,
        int boardId,
        int sortOrder,
        int possessionGroupId = 0,
        int effectGroupId = 0)
    {
        masterDb.EntityMCharacterBoardPanel.Add(new EntityMCharacterBoardPanel
        {
            CharacterBoardPanelId = panelId,
            CharacterBoardId = boardId,
            CharacterBoardPanelReleasePossessionGroupId = possessionGroupId,
            CharacterBoardPanelReleaseEffectGroupId = effectGroupId,
            SortOrder = sortOrder
        });
    }

    private static void AddCost(
        DarkMasterMemoryDatabase masterDb,
        int groupId,
        PossessionType possessionType,
        int possessionId,
        int count,
        int sortOrder)
    {
        masterDb.EntityMCharacterBoardPanelReleasePossessionGroup.Add(new EntityMCharacterBoardPanelReleasePossessionGroup
        {
            CharacterBoardPanelReleasePossessionGroupId = groupId,
            PossessionType = possessionType,
            PossessionId = possessionId,
            Count = count,
            SortOrder = sortOrder
        });
    }

    private static void AddAbilityEffect(
        DarkMasterMemoryDatabase masterDb,
        int effectGroupId,
        int characterBoardAbilityId,
        int effectValue)
    {
        masterDb.EntityMCharacterBoardPanelReleaseEffectGroup.Add(new EntityMCharacterBoardPanelReleaseEffectGroup
        {
            CharacterBoardPanelReleaseEffectGroupId = effectGroupId,
            SortOrder = masterDb.EntityMCharacterBoardPanelReleaseEffectGroup.Count + 1,
            CharacterBoardEffectType = CharacterBoardEffectType.ABILITY,
            CharacterBoardEffectId = characterBoardAbilityId,
            EffectValue = effectValue
        });
    }

    private static void AddStatusEffect(
        DarkMasterMemoryDatabase masterDb,
        int effectGroupId,
        int characterBoardStatusUpId,
        int effectValue)
    {
        masterDb.EntityMCharacterBoardPanelReleaseEffectGroup.Add(new EntityMCharacterBoardPanelReleaseEffectGroup
        {
            CharacterBoardPanelReleaseEffectGroupId = effectGroupId,
            SortOrder = masterDb.EntityMCharacterBoardPanelReleaseEffectGroup.Count + 1,
            CharacterBoardEffectType = CharacterBoardEffectType.STATUS_UP,
            CharacterBoardEffectId = characterBoardStatusUpId,
            EffectValue = effectValue
        });
    }

    private static void AddTargetGroup(DarkMasterMemoryDatabase masterDb, int targetGroupId, int characterId)
    {
        masterDb.EntityMCharacterBoardEffectTargetGroup.Add(new EntityMCharacterBoardEffectTargetGroup
        {
            CharacterBoardEffectTargetGroupId = targetGroupId,
            TargetValue = characterId
        });
    }

    [Fact]
    public async Task ReleasePanel_DeductsCostsSetsReleaseBitAndAppliesEffects()
    {
        const int panelId = 1001;
        const int boardId = 2001;
        const int costGroupId = 3001;
        const int effectGroupId = 4001;
        const int materialId = 5001;
        const int consumableItemId = 6001;
        const int characterId = 7001;
        const int characterBoardAbilityId = 8001;
        const int abilityId = 9001;
        const int abilityTargetGroupId = 9101;
        const int characterBoardStatusUpId = 8201;
        const int statusTargetGroupId = 9201;

        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddPanel(masterDb, panelId, boardId, sortOrder: 1, possessionGroupId: costGroupId, effectGroupId: effectGroupId);
        AddCost(masterDb, costGroupId, PossessionType.MATERIAL, materialId, count: 2, sortOrder: 1);
        AddCost(masterDb, costGroupId, PossessionType.CONSUMABLE_ITEM, consumableItemId, count: 3, sortOrder: 2);
        AddCost(masterDb, costGroupId, PossessionType.PAID_GEM, possessionId: 0, count: 5, sortOrder: 3);
        AddCost(masterDb, costGroupId, PossessionType.FREE_GEM, possessionId: 0, count: 7, sortOrder: 4);
        AddAbilityEffect(masterDb, effectGroupId, characterBoardAbilityId, effectValue: 5);
        AddStatusEffect(masterDb, effectGroupId, characterBoardStatusUpId, effectValue: 11);
        masterDb.EntityMCharacterBoardAbility.Add(new EntityMCharacterBoardAbility
        {
            CharacterBoardAbilityId = characterBoardAbilityId,
            CharacterBoardEffectTargetGroupId = abilityTargetGroupId,
            AbilityId = abilityId
        });
        masterDb.EntityMCharacterBoardAbilityMaxLevel.Add(new EntityMCharacterBoardAbilityMaxLevel
        {
            CharacterId = characterId,
            AbilityId = abilityId,
            MaxLevel = 4
        });
        masterDb.EntityMCharacterBoardStatusUp.Add(new EntityMCharacterBoardStatusUp
        {
            CharacterBoardStatusUpId = characterBoardStatusUpId,
            CharacterBoardStatusUpType = CharacterBoardStatusUpType.ATTACK_ADD,
            CharacterBoardEffectTargetGroupId = statusTargetGroupId
        });
        AddTargetGroup(masterDb, abilityTargetGroupId, characterId);
        AddTargetGroup(masterDb, statusTargetGroupId, characterId);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = materialId, Count = 2 });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem { UserId = UserId, ConsumableItemId = consumableItemId, Count = 5 });
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, PaidGem = 100, FreeGem = 50 });
        userDb.EntityIUserCharacterBoardAbility.Add(new EntityIUserCharacterBoardAbility
        {
            UserId = UserId,
            CharacterId = characterId,
            AbilityId = abilityId,
            Level = 2
        });
        userDb.EntityIUserCharacterBoardStatusUp.Add(new EntityIUserCharacterBoardStatusUp
        {
            UserId = UserId,
            CharacterId = characterId,
            StatusCalculationType = StatusCalculationType.ADD,
            Attack = 4
        });

        UserDataStore store = CreateStore(UserId, userDb, masterDb);
        CharacterBoardService service = CreateService(masterDb, store);

        await service.ReleasePanel(
            new ReleasePanelRequest { CharacterBoardPanelId = { panelId } },
            ContextFor(UserId));

        Assert.Empty(userDb.EntityIUserMaterial);
        EntityIUserConsumableItem consumableItem = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(2, consumableItem.Count);

        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(95, gem.PaidGem);
        Assert.Equal(43, gem.FreeGem);

        EntityIUserCharacterBoard board = Assert.Single(userDb.EntityIUserCharacterBoard);
        Assert.Equal(boardId, board.CharacterBoardId);
        Assert.Equal(1, board.PanelReleaseBit1);
        Assert.Equal(0, board.PanelReleaseBit2);
        Assert.Equal(0, board.PanelReleaseBit3);
        Assert.Equal(0, board.PanelReleaseBit4);

        EntityIUserCharacterBoardAbility abilityState = Assert.Single(userDb.EntityIUserCharacterBoardAbility);
        Assert.Equal(4, abilityState.Level);

        EntityIUserCharacterBoardStatusUp statusUpState = Assert.Single(userDb.EntityIUserCharacterBoardStatusUp);
        Assert.Equal(StatusCalculationType.ADD, statusUpState.StatusCalculationType);
        Assert.Equal(15, statusUpState.Attack);
    }

    [Fact]
    public async Task ReleasePanel_WithMultiplePanels_ReusesExistingBoardAndCreatesNewStates()
    {
        const int boardId = 2002;
        const int characterId = 7002;
        const int multiplyStatusPanelId = 1002;
        const int abilityPanelId = 1003;
        const int multiplyStatusEffectGroupId = 4002;
        const int abilityEffectGroupId = 4003;
        const int characterBoardStatusUpId = 8202;
        const int statusTargetGroupId = 9202;
        const int characterBoardAbilityId = 8002;
        const int abilityId = 9002;
        const int abilityTargetGroupId = 9102;

        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddPanel(masterDb, multiplyStatusPanelId, boardId, sortOrder: 33, effectGroupId: multiplyStatusEffectGroupId);
        AddPanel(masterDb, abilityPanelId, boardId, sortOrder: 34, effectGroupId: abilityEffectGroupId);
        AddStatusEffect(masterDb, multiplyStatusEffectGroupId, characterBoardStatusUpId, effectValue: 9);
        AddAbilityEffect(masterDb, abilityEffectGroupId, characterBoardAbilityId, effectValue: 2);
        masterDb.EntityMCharacterBoardStatusUp.Add(new EntityMCharacterBoardStatusUp
        {
            CharacterBoardStatusUpId = characterBoardStatusUpId,
            CharacterBoardStatusUpType = CharacterBoardStatusUpType.HP_MULTIPLY,
            CharacterBoardEffectTargetGroupId = statusTargetGroupId
        });
        masterDb.EntityMCharacterBoardAbility.Add(new EntityMCharacterBoardAbility
        {
            CharacterBoardAbilityId = characterBoardAbilityId,
            CharacterBoardEffectTargetGroupId = abilityTargetGroupId,
            AbilityId = abilityId
        });
        AddTargetGroup(masterDb, statusTargetGroupId, characterId);
        AddTargetGroup(masterDb, abilityTargetGroupId, characterId);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserCharacterBoard existingBoard = new()
        {
            UserId = UserId,
            CharacterBoardId = boardId,
            PanelReleaseBit1 = 4
        };
        userDb.EntityIUserCharacterBoard.Add(existingBoard);

        UserDataStore store = CreateStore(UserId, userDb, masterDb);
        CharacterBoardService service = CreateService(masterDb, store);

        await service.ReleasePanel(
            new ReleasePanelRequest { CharacterBoardPanelId = { multiplyStatusPanelId, abilityPanelId } },
            ContextFor(UserId));

        EntityIUserCharacterBoard board = Assert.Single(userDb.EntityIUserCharacterBoard);
        Assert.Same(existingBoard, board);
        Assert.Equal(4, board.PanelReleaseBit1);
        Assert.Equal(3, board.PanelReleaseBit2);
        Assert.Equal(0, board.PanelReleaseBit3);
        Assert.Equal(0, board.PanelReleaseBit4);

        EntityIUserCharacterBoardStatusUp statusUpState = Assert.Single(userDb.EntityIUserCharacterBoardStatusUp);
        Assert.Equal(characterId, statusUpState.CharacterId);
        Assert.Equal(StatusCalculationType.MULTIPLY, statusUpState.StatusCalculationType);
        Assert.Equal(9, statusUpState.Hp);

        EntityIUserCharacterBoardAbility abilityState = Assert.Single(userDb.EntityIUserCharacterBoardAbility);
        Assert.Equal(characterId, abilityState.CharacterId);
        Assert.Equal(abilityId, abilityState.AbilityId);
        Assert.Equal(2, abilityState.Level);
    }

    [Fact]
    public async Task ReleasePanel_UnknownPanelsAndUnresolvableEffects_AreIgnored()
    {
        const int validPanelId = 1004;
        const int boardId = 2003;
        const int costGroupId = 3004;
        const int effectGroupId = 4004;
        const int materialId = 5004;
        const int abilityWithoutTargetId = 8004;
        const int emptyTargetGroupId = 9104;

        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddPanel(masterDb, validPanelId, boardId, sortOrder: 2, possessionGroupId: costGroupId, effectGroupId: effectGroupId);
        AddCost(masterDb, costGroupId, PossessionType.MATERIAL, materialId, count: 1, sortOrder: 1);
        AddAbilityEffect(masterDb, effectGroupId, characterBoardAbilityId: 9999, effectValue: 3);
        AddStatusEffect(masterDb, effectGroupId, characterBoardStatusUpId: 8888, effectValue: 4);
        AddAbilityEffect(masterDb, effectGroupId, abilityWithoutTargetId, effectValue: 2);
        masterDb.EntityMCharacterBoardAbility.Add(new EntityMCharacterBoardAbility
        {
            CharacterBoardAbilityId = abilityWithoutTargetId,
            CharacterBoardEffectTargetGroupId = emptyTargetGroupId,
            AbilityId = 9004
        });
        AddTargetGroup(masterDb, emptyTargetGroupId, characterId: 0);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = materialId, Count = 1 });

        UserDataStore store = CreateStore(UserId, userDb, masterDb);
        CharacterBoardService service = CreateService(masterDb, store);

        await service.ReleasePanel(
            new ReleasePanelRequest { CharacterBoardPanelId = { 999_999, validPanelId } },
            ContextFor(UserId));

        Assert.Empty(userDb.EntityIUserMaterial);

        EntityIUserCharacterBoard board = Assert.Single(userDb.EntityIUserCharacterBoard);
        Assert.Equal(boardId, board.CharacterBoardId);
        Assert.Equal(2, board.PanelReleaseBit1);

        Assert.Empty(userDb.EntityIUserCharacterBoardAbility);
        Assert.Empty(userDb.EntityIUserCharacterBoardStatusUp);
    }
}
