using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.CharacterViewer;

namespace MariesWonderland.Services;

public class CharacterViewerService(UserDataStore store, DarkMasterMemoryDatabase masterDb)
    : MariesWonderland.Proto.CharacterViewer.CharacterviewerService.CharacterviewerServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Returns the character viewer top screen with all unlocked field IDs based on quest clear conditions.</summary>
    public override Task<CharacterViewerTopResponse> CharacterViewerTop(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        Dictionary<int, long> conditionToQuestId = BuildConditionQuestMap();

        // Collect cleared quest IDs to evaluate unlock prerequisites
        HashSet<int> clearedQuests = [];
        foreach (EntityIUserQuest quest in userDb.EntityIUserQuest)
        {
            if (quest.QuestStateType == (int)QuestStateType.CLEARED)
            {
                clearedQuests.Add(quest.QuestId);
            }
        }

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        List<int> releasedFieldIds = [];

        // Check each field's release condition and track newly unlocked ones
        foreach (EntityMCharacterViewerField field in _masterDb.EntityMCharacterViewerField)
        {
            bool isReleased;
            if (field.ReleaseEvaluateConditionId == 0)
            {
                isReleased = true;
            }
            else if (conditionToQuestId.TryGetValue(field.ReleaseEvaluateConditionId, out long requiredQuestId))
            {
                isReleased = clearedQuests.Contains((int)requiredQuestId);
            }
            else
            {
                isReleased = false;
            }

            if (!isReleased)
            {
                continue;
            }

            releasedFieldIds.Add(field.CharacterViewerFieldId);

            bool alreadyTracked = userDb.EntityIUserCharacterViewerField
                .Any(f => f.CharacterViewerFieldId == field.CharacterViewerFieldId);

            if (!alreadyTracked)
            {
                userDb.EntityIUserCharacterViewerField.Add(new EntityIUserCharacterViewerField
                {
                    UserId = userId,
                    CharacterViewerFieldId = field.CharacterViewerFieldId,
                    ReleaseDatetime = nowMs
                });
            }
        }

        releasedFieldIds.Sort();

        CharacterViewerTopResponse response = new();
        response.ReleaseCharacterViewerFieldId.AddRange(releasedFieldIds);

        return Task.FromResult(response);
    }

    /// <summary>Builds a mapping from EvaluateConditionId to required quest ID for QUEST_CLEAR conditions.</summary>
    private Dictionary<int, long> BuildConditionQuestMap()
    {
        Dictionary<(int GroupId, int GroupIndex), long> vgByKey = [];
        foreach (EntityMEvaluateConditionValueGroup vg in _masterDb.EntityMEvaluateConditionValueGroup)
        {
            vgByKey[(vg.EvaluateConditionValueGroupId, vg.GroupIndex)] = vg.Value;
        }

        Dictionary<int, long> conditionToQuestId = [];
        foreach (EntityMEvaluateCondition cond in _masterDb.EntityMEvaluateCondition)
        {
            if (cond.EvaluateConditionFunctionType != EvaluateConditionFunctionType.QUEST_CLEAR)
            {
                continue;
            }
            if (cond.EvaluateConditionEvaluateType != EvaluateConditionEvaluateType.ID_CONTAIN)
            {
                continue;
            }
            if (vgByKey.TryGetValue((cond.EvaluateConditionValueGroupId, 1), out long questId))
            {
                conditionToQuestId[cond.EvaluateConditionId] = questId;
            }
        }

        return conditionToQuestId;
    }
}

