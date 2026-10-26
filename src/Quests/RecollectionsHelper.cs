using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Quests;

/// <summary>
/// Recollections of Dusk floors. A floor is one chapter at one difficulty: the quests of one event quest sequence. Clearing
/// a floor quest locks the costumes (and, where the floor says so, the weapons) of the deck used for the floor's other
/// quests (<c>IUserDeckLimitContentRestricted</c>, which the client reads when building decks). The locks lift when every
/// quest of the floor is cleared; a reset starts the floor over.
/// </summary>
public static class RecollectionsHelper
{
    /// <summary>Whether the event chapter is a Recollections of Dusk one.</summary>
    public static bool IsRecollectionsChapter(DarkMasterMemoryDatabase masterDb, int eventQuestChapterId)
        => masterDb.EntityMEventQuestChapter.FirstOrDefault(c => c.EventQuestChapterId == eventQuestChapterId)?.EventQuestType == EventQuestType.LIMIT_CONTENT;

    /// <summary>The quests of the floor <paramref name="questId"/> belongs to in the chapter (empty when it isn't one of its quests).</summary>
    public static IReadOnlySet<int> FloorQuestIds(DarkMasterMemoryDatabase masterDb, int eventQuestChapterId, int questId)
    {
        int groupId = masterDb.EntityMEventQuestChapter.FirstOrDefault(c => c.EventQuestChapterId == eventQuestChapterId)?.EventQuestSequenceGroupId ?? 0;
        HashSet<int> sequenceIds = [.. masterDb.EntityMEventQuestSequenceGroup
            .Where(g => g.EventQuestSequenceGroupId == groupId)
            .Select(g => g.EventQuestSequenceId)];
        int sequenceId = masterDb.EntityMEventQuestSequence
            .FirstOrDefault(s => sequenceIds.Contains(s.EventQuestSequenceId) && s.QuestId == questId)?.EventQuestSequenceId ?? 0;
        return sequenceId == 0
            ? new HashSet<int>()
            : [.. masterDb.EntityMEventQuestSequence.Where(s => s.EventQuestSequenceId == sequenceId).Select(s => s.QuestId)];
    }

    /// <summary>
    /// What a clear on the chapter's floors locks right now: chapter → limit content → its deck restriction (the rows
    /// active at <paramref name="nowMs"/>) → their targets.
    /// </summary>
    public static IReadOnlySet<LimitContentDeckRestrictionType> RestrictedTypes(DarkMasterMemoryDatabase masterDb, int eventQuestChapterId, long nowMs)
    {
        HashSet<int> contentIds = [.. masterDb.EntityMEventQuestChapterLimitContentRelation
            .Where(r => r.EventQuestChapterId == eventQuestChapterId)
            .Select(r => r.EventQuestLimitContentId)];
        HashSet<int> restrictionIds = [.. masterDb.EntityMEventQuestLimitContent
            .Where(c => contentIds.Contains(c.EventQuestLimitContentId))
            .Select(c => c.EventQuestLimitContentDeckRestrictionId)];
        HashSet<int> targetIds = [.. masterDb.EntityMEventQuestLimitContentDeckRestriction
            .Where(r => restrictionIds.Contains(r.EventQuestLimitContentDeckRestrictionId) && nowMs >= r.StartDatetime && nowMs <= r.EndDatetime)
            .Select(r => r.EventQuestLimitContentDeckRestrictionTargetId)];
        return masterDb.EntityMEventQuestLimitContentDeckRestrictionTarget
            .Where(t => targetIds.Contains(t.EventQuestLimitContentDeckRestrictionTargetId))
            .Select(t => t.LimitContentDeckRestrictionType)
            .ToHashSet();
    }

    /// <summary>
    /// A won floor quest: marks it cleared, locks the restricted items of <paramref name="deck"/> (the deck the run used)
    /// for the floor, and lifts the floor's locks once all of its quests are cleared.
    /// </summary>
    public static void RecordClear(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, int eventQuestChapterId, int questId, EntityIUserDeck? deck, long nowMs)
    {
        EntityIUserQuestLimitContentStatus status = userDb.EntityIUserQuestLimitContentStatus.GetOrCreate(
            s => s.QuestId == questId,
            () => new EntityIUserQuestLimitContentStatus { UserId = userDb.UserId, QuestId = questId });
        status.LimitContentQuestStatusType = 1;
        status.EventQuestChapterId = eventQuestChapterId;

        if (deck != null)
        {
            foreach ((PossessionType possessionType, string uuid) in RestrictedItems(userDb, deck, RestrictedTypes(masterDb, eventQuestChapterId, nowMs)))
            {
                bool alreadyLocked = userDb.EntityIUserDeckLimitContentRestricted.Any(r =>
                    r.EventQuestChapterId == eventQuestChapterId && r.QuestId == questId && r.PossessionType == possessionType && r.TargetUuid == uuid);
                if (!alreadyLocked)
                {
                    userDb.EntityIUserDeckLimitContentRestricted.Add(new EntityIUserDeckLimitContentRestricted
                    {
                        UserId = userDb.UserId,
                        EventQuestChapterId = eventQuestChapterId,
                        QuestId = questId,
                        DeckRestrictedUuid = Guid.NewGuid().ToString(),
                        PossessionType = possessionType,
                        TargetUuid = uuid
                    });
                }
            }
        }

        IReadOnlySet<int> floor = FloorQuestIds(masterDb, eventQuestChapterId, questId);
        bool floorComplete = floor.Count > 0 && floor.All(id =>
            userDb.EntityIUserQuestLimitContentStatus.Any(s => s.QuestId == id && s.LimitContentQuestStatusType == 1));
        if (floorComplete)
        {
            RemoveLocks(userDb, eventQuestChapterId, floor);
        }
    }

    /// <summary>Starts the floor of <paramref name="questId"/> over: none of its quests cleared, nothing locked.</summary>
    public static void ResetFloor(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, int eventQuestChapterId, int questId)
    {
        IReadOnlySet<int> floor = FloorQuestIds(masterDb, eventQuestChapterId, questId);
        if (floor.Count == 0)
        {
            floor = new HashSet<int> { questId };
        }

        userDb.EntityIUserQuestLimitContentStatus.RemoveAll(s => floor.Contains(s.QuestId));
        RemoveLocks(userDb, eventQuestChapterId, floor);
    }

    private static void RemoveLocks(DarkUserMemoryDatabase userDb, int eventQuestChapterId, IReadOnlySet<int> floorQuestIds)
        => userDb.EntityIUserDeckLimitContentRestricted.RemoveAll(r => r.EventQuestChapterId == eventQuestChapterId && floorQuestIds.Contains(r.QuestId));

    /// <summary>The deck's costumes and weapons (main and sub) of the restricted types, as (possession type, user uuid).</summary>
    private static IEnumerable<(PossessionType, string)> RestrictedItems(DarkUserMemoryDatabase userDb, EntityIUserDeck deck, IReadOnlySet<LimitContentDeckRestrictionType> types)
    {
        foreach (string? deckCharacterUuid in new[] { deck.UserDeckCharacterUuid01, deck.UserDeckCharacterUuid02, deck.UserDeckCharacterUuid03 })
        {
            EntityIUserDeckCharacter? character = string.IsNullOrEmpty(deckCharacterUuid)
                ? null
                : userDb.EntityIUserDeckCharacter.FirstOrDefault(c => c.UserDeckCharacterUuid == deckCharacterUuid);
            if (character == null)
            {
                continue;
            }

            if (types.Contains(LimitContentDeckRestrictionType.COSTUME) && !string.IsNullOrEmpty(character.UserCostumeUuid))
            {
                yield return (PossessionType.COSTUME, character.UserCostumeUuid);
            }

            if (types.Contains(LimitContentDeckRestrictionType.WEAPON))
            {
                if (!string.IsNullOrEmpty(character.MainUserWeaponUuid))
                {
                    yield return (PossessionType.WEAPON, character.MainUserWeaponUuid);
                }

                foreach (EntityIUserDeckSubWeaponGroup sub in userDb.EntityIUserDeckSubWeaponGroup
                    .Where(s => s.UserDeckCharacterUuid == character.UserDeckCharacterUuid && !string.IsNullOrEmpty(s.UserWeaponUuid))
                    .OrderBy(s => s.SortOrder))
                {
                    yield return (PossessionType.WEAPON, sub.UserWeaponUuid);
                }
            }
        }
    }
}
