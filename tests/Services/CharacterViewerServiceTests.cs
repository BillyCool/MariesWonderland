using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.CharacterViewer;
using MariesWonderland.Tests.Infrastructure;
using CharacterViewerServiceImpl = MariesWonderland.Services.CharacterViewerService;

namespace MariesWonderland.Tests.Services;

public class CharacterViewerServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task CharacterViewerTop_ReturnsSortedReleasedFieldsAndPersistsNewUnlocks()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddField(masterDb, fieldId: 20, releaseConditionId: 0);
        AddField(masterDb, fieldId: 10, releaseConditionId: 100);
        AddQuestClearCondition(masterDb, conditionId: 100, requiredQuestId: 3001);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        AddQuest(userDb, questId: 3001, QuestStateType.CLEARED);
        userDb.EntityIUserCharacterViewerField.Add(new EntityIUserCharacterViewerField
        {
            UserId = UserId,
            CharacterViewerFieldId = 20,
            ReleaseDatetime = 123
        });

        CharacterViewerServiceImpl service = CreateService(masterDb, userDb);

        CharacterViewerTopResponse response = await service.CharacterViewerTop(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal([10, 20], response.ReleaseCharacterViewerFieldId);

        EntityIUserCharacterViewerField existingField = Assert.Single(
            userDb.EntityIUserCharacterViewerField,
            f => f.CharacterViewerFieldId == 20);
        Assert.Equal(123, existingField.ReleaseDatetime);

        EntityIUserCharacterViewerField newlyTrackedField = Assert.Single(
            userDb.EntityIUserCharacterViewerField,
            f => f.CharacterViewerFieldId == 10);
        Assert.Equal(UserId, newlyTrackedField.UserId);
        Assert.True(newlyTrackedField.ReleaseDatetime > 0);
        Assert.Equal(2, userDb.EntityIUserCharacterViewerField.Count);
    }

    [Fact]
    public async Task CharacterViewerTop_OnlyUnlocksFieldsWithSupportedClearedQuestConditions()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddField(masterDb, fieldId: 1, releaseConditionId: 0);
        AddField(masterDb, fieldId: 2, releaseConditionId: 101);
        AddField(masterDb, fieldId: 3, releaseConditionId: 102);
        AddField(masterDb, fieldId: 4, releaseConditionId: 103);
        AddField(masterDb, fieldId: 5, releaseConditionId: 104);
        AddField(masterDb, fieldId: 6, releaseConditionId: 999);

        AddQuestClearCondition(masterDb, conditionId: 101, requiredQuestId: 4001);
        AddCondition(masterDb, conditionId: 102, EvaluateConditionFunctionType.QUEST_CLEAR, EvaluateConditionEvaluateType.EQ, valueGroupId: 502);
        AddValueGroup(masterDb, valueGroupId: 502, groupIndex: 1, value: 4002);
        AddCondition(masterDb, conditionId: 103, EvaluateConditionFunctionType.MISSION_CLEAR, EvaluateConditionEvaluateType.ID_CONTAIN, valueGroupId: 503);
        AddValueGroup(masterDb, valueGroupId: 503, groupIndex: 1, value: 4003);
        AddCondition(masterDb, conditionId: 104, EvaluateConditionFunctionType.QUEST_CLEAR, EvaluateConditionEvaluateType.ID_CONTAIN, valueGroupId: 504);
        AddValueGroup(masterDb, valueGroupId: 504, groupIndex: 2, value: 4004);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        AddQuest(userDb, questId: 4001, QuestStateType.ACTIVE);
        AddQuest(userDb, questId: 4002, QuestStateType.CLEARED);
        AddQuest(userDb, questId: 4003, QuestStateType.CLEARED);
        AddQuest(userDb, questId: 4004, QuestStateType.CLEARED);

        CharacterViewerServiceImpl service = CreateService(masterDb, userDb);

        CharacterViewerTopResponse response = await service.CharacterViewerTop(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal([1], response.ReleaseCharacterViewerFieldId);
        EntityIUserCharacterViewerField releasedField = Assert.Single(userDb.EntityIUserCharacterViewerField);
        Assert.Equal(1, releasedField.CharacterViewerFieldId);
    }

    [Fact]
    public async Task CharacterViewerTop_RepeatedCallsDoNotCreateDuplicateUnlockRows()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddField(masterDb, fieldId: 7, releaseConditionId: 0);
        AddField(masterDb, fieldId: 9, releaseConditionId: 105);
        AddQuestClearCondition(masterDb, conditionId: 105, requiredQuestId: 5001);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        AddQuest(userDb, questId: 5001, QuestStateType.CLEARED);

        CharacterViewerServiceImpl service = CreateService(masterDb, userDb);

        CharacterViewerTopResponse firstResponse = await service.CharacterViewerTop(
            new Empty(),
            FakeServerCallContext.For(UserId));
        CharacterViewerTopResponse secondResponse = await service.CharacterViewerTop(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal([7, 9], firstResponse.ReleaseCharacterViewerFieldId);
        Assert.Equal([7, 9], secondResponse.ReleaseCharacterViewerFieldId);
        Assert.Equal(2, userDb.EntityIUserCharacterViewerField.Count);
        Assert.Single(userDb.EntityIUserCharacterViewerField, f => f.CharacterViewerFieldId == 7);
        Assert.Single(userDb.EntityIUserCharacterViewerField, f => f.CharacterViewerFieldId == 9);
    }

    [Fact]
    public async Task CharacterViewerTop_WithoutExistingUserDatabase_CreatesOneAndTracksAlwaysReleasedFields()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddField(masterDb, fieldId: 42, releaseConditionId: 0);
        UserDataStore store = new(masterDb);
        CharacterViewerServiceImpl service = new(store, masterDb);

        CharacterViewerTopResponse response = await service.CharacterViewerTop(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal([42], response.ReleaseCharacterViewerFieldId);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);
        Assert.Equal(UserId, userDb.UserId);

        EntityIUserCharacterViewerField releasedField = Assert.Single(userDb.EntityIUserCharacterViewerField);
        Assert.Equal(42, releasedField.CharacterViewerFieldId);
        Assert.Equal(UserId, releasedField.UserId);
        Assert.True(releasedField.ReleaseDatetime > 0);
    }

    private static CharacterViewerServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(masterDb);
        store.Set(UserId, userDb);
        return new CharacterViewerServiceImpl(store, masterDb);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void AddField(DarkMasterMemoryDatabase masterDb, int fieldId, int releaseConditionId)
    {
        masterDb.EntityMCharacterViewerField.Add(new EntityMCharacterViewerField
        {
            CharacterViewerFieldId = fieldId,
            ReleaseEvaluateConditionId = releaseConditionId
        });
    }

    private static void AddQuest(DarkUserMemoryDatabase userDb, int questId, QuestStateType stateType)
    {
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = questId,
            QuestStateType = (int)stateType
        });
    }

    private static void AddQuestClearCondition(DarkMasterMemoryDatabase masterDb, int conditionId, long requiredQuestId)
    {
        int valueGroupId = conditionId + 1000;
        AddCondition(masterDb, conditionId, EvaluateConditionFunctionType.QUEST_CLEAR, EvaluateConditionEvaluateType.ID_CONTAIN, valueGroupId);
        AddValueGroup(masterDb, valueGroupId, groupIndex: 1, value: requiredQuestId);
    }

    private static void AddCondition(
        DarkMasterMemoryDatabase masterDb,
        int conditionId,
        EvaluateConditionFunctionType functionType,
        EvaluateConditionEvaluateType evaluateType,
        int valueGroupId)
    {
        masterDb.EntityMEvaluateCondition.Add(new EntityMEvaluateCondition
        {
            EvaluateConditionId = conditionId,
            EvaluateConditionFunctionType = functionType,
            EvaluateConditionEvaluateType = evaluateType,
            EvaluateConditionValueGroupId = valueGroupId
        });
    }

    private static void AddValueGroup(DarkMasterMemoryDatabase masterDb, int valueGroupId, int groupIndex, long value)
    {
        masterDb.EntityMEvaluateConditionValueGroup.Add(new EntityMEvaluateConditionValueGroup
        {
            EvaluateConditionValueGroupId = valueGroupId,
            GroupIndex = groupIndex,
            Value = value
        });
    }
}
