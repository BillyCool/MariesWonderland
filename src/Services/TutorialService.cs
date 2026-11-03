using Grpc.Core;
using MariesWonderland.Constants;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Deck;
using MariesWonderland.Proto.Tutorial;

namespace MariesWonderland.Services;

public class TutorialService(UserDataStore store, DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, TimeProvider timeProvider) : MariesWonderland.Proto.Tutorial.TutorialService.TutorialServiceBase
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>The choice a story step sends when the player takes the item or says yes (the real save holds it on 1901-1906).</summary>
    private const int AcceptedChoiceId = 1;

    /// <summary>
    /// Skip tickets picked up in the Main Quest 3-3 story (c030). The event maps send 1901 in scene 250 ("Skip Ticket
    /// x20") and 1902 in scene 290 ("x10"), both with choice 1 and the "obtained" popup. 1903-1905 are three books in
    /// the library map (scene 280) that give 10 each (seen in a first-playthrough recording).
    /// </summary>
    private static readonly Dictionary<TutorialType, int> SkipTicketPickups = new()
    {
        [TutorialType.MAIN_QUEST_3_3_TICKET_GET_1] = 20,
        [TutorialType.MAIN_QUEST_3_3_TICKET_GET_2] = 10,
        [TutorialType.MAIN_QUEST_3_3_TICKET_GET_3] = 10,
        [TutorialType.MAIN_QUEST_3_3_TICKET_GET_4] = 10,
        [TutorialType.MAIN_QUEST_3_3_TICKET_GET_5] = 10
    };

    /// <summary>The phase of the companion tutorial that carries the choice (the starter companions are in <see cref="StarterCompanionTable"/>).</summary>
    private const int CompanionChoicePhase = 10;

    /// <summary>
    /// Advances tutorial progress and triggers starter deck creation on menu tutorials. The first time a story step is
    /// accepted, it gives its pickup (returned as choice rewards) or uses the items its consume rows list (one skip
    /// ticket to clear the 3-3 barrier). A replay sends the same choice again and changes nothing.
    /// </summary>
    public override Task<SetTutorialProgressResponse> SetTutorialProgress(SetTutorialProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        TutorialType tutorialType = (TutorialType)request.TutorialType;

        EntityIUserTutorialProgress? previousProgress = userDb.EntityIUserTutorialProgress.FirstOrDefault(t => t.TutorialType == tutorialType);
        int previousChoiceId = previousProgress?.ChoiceId ?? 0;
        bool isFirstCompanionChoice = tutorialType == TutorialType.COMPANION && request.ProgressPhase == CompanionChoicePhase
            && (previousProgress?.ProgressPhase ?? 0) < CompanionChoicePhase;
        UpsertTutorialProgress(userDb, tutorialType, request.ProgressPhase, request.ChoiceId);

        if (tutorialType is TutorialType.MENU_FIRST or TutorialType.MENU_SECOND)
        {
            CreateStarterDeck(userDb);
        }

        SetTutorialProgressResponse response = new();
        if (isFirstCompanionChoice && StarterCompanionTable.Default.TryGetCompanionId(request.ChoiceId, out int companionId))
        {
            PossessionHelper.Apply(userDb, PossessionType.COMPANION, companionId, 1, masterDb, nowMs);
            response.TutorialChoiceReward.Add(new TutorialChoiceReward { PossessionType = (int)PossessionType.COMPANION, PossessionId = companionId, Count = 1 });
        }
        else if (request.ChoiceId == AcceptedChoiceId && previousChoiceId != AcceptedChoiceId)
        {
            response.TutorialChoiceReward.AddRange(GrantPickup(userDb, tutorialType, nowMs));
            ConsumeTutorialPossessions(userDb, tutorialType);
        }

        return Task.FromResult(response);
    }

    /// <summary>Advances tutorial progress and replaces the user's deck in one operation.</summary>
    public override Task<SetTutorialProgressAndReplaceDeckResponse> SetTutorialProgressAndReplaceDeck(SetTutorialProgressAndReplaceDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(userId);

        UpsertTutorialProgress(userDb, (TutorialType)request.TutorialType, request.ProgressPhase, choiceId: 0);
        UpsertDeck(userDb, request);

        return Task.FromResult(new SetTutorialProgressAndReplaceDeckResponse());
    }

    /// <summary>Creates or updates a tutorial progress record, advancing to the specified phase.</summary>
    private static void UpsertTutorialProgress(DarkUserMemoryDatabase db, TutorialType type, int phase, int choiceId)
    {
        EntityIUserTutorialProgress? existing = db.EntityIUserTutorialProgress
            .FirstOrDefault(t => t.TutorialType == type);

        if (existing is null)
        {
            db.EntityIUserTutorialProgress.Add(new EntityIUserTutorialProgress
            {
                UserId = db.UserId,
                TutorialType = type,
                ProgressPhase = phase,
                ChoiceId = choiceId
            });
        }
        else
        {
            if (phase >= existing.ProgressPhase)
            {
                existing.ProgressPhase = phase;
                existing.ChoiceId = choiceId;
            }
        }
    }

    /// <summary>
    /// Creates a minimal starter deck (DeckType=QUEST, DeckNumber=1) of Rion and Everlasting Cardia. Only runs when no
    /// deck 1 character slot exists yet. Idempotent: safe to call multiple times.
    /// Everlasting Cardia is a first-clear reward of the first Main Quest, so a player who opens the menu before
    /// clearing it doesn't own it yet. No deck is made then; the next menu tutorial after the clear makes it.
    /// </summary>
    private static void CreateStarterDeck(DarkUserMemoryDatabase db)
    {
        EntityIUserDeck? existingDeck = db.EntityIUserDeck
            .FirstOrDefault(d => d.DeckType == DeckType.QUEST && d.UserDeckNumber == 1);

        if (existingDeck is not null && !string.IsNullOrEmpty(existingDeck.UserDeckCharacterUuid01))
        {
            return;
        }

        string? costumeUuid = db.EntityIUserCostume
            .FirstOrDefault(c => c.CostumeId == GameConstants.StartingDeckCostumeId)?.UserCostumeUuid;

        string? weaponUuid = db.EntityIUserWeapon
            .FirstOrDefault(w => w.WeaponId == GameConstants.StartingDeckWeaponId)?.UserWeaponUuid;

        if (costumeUuid is null || weaponUuid is null)
        {
            return;
        }

        string dcUuid = Guid.NewGuid().ToString();

        db.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter
        {
            UserId = db.UserId,
            UserDeckCharacterUuid = dcUuid,
            UserCostumeUuid = costumeUuid,
            MainUserWeaponUuid = weaponUuid,
            UserCompanionUuid = "",
            UserThoughtUuid = "",
            Power = 0
        });

        if (existingDeck is null)
        {
            db.EntityIUserDeck.Add(new EntityIUserDeck
            {
                UserId = db.UserId,
                DeckType = DeckType.QUEST,
                UserDeckNumber = 1,
                UserDeckCharacterUuid01 = dcUuid,
                UserDeckCharacterUuid02 = "",
                UserDeckCharacterUuid03 = "",
                Name = "Loadout 1",
                Power = 0
            });
        }
        else
        {
            existingDeck.UserDeckCharacterUuid01 = dcUuid;
        }

        bool hasDeckTypeNote = db.EntityIUserDeckTypeNote
            .Any(n => n.DeckType == DeckType.QUEST);

        if (!hasDeckTypeNote)
        {
            db.EntityIUserDeckTypeNote.Add(new EntityIUserDeckTypeNote
            {
                UserId = db.UserId,
                DeckType = DeckType.QUEST,
                MaxDeckPower = 0
            });
        }
    }

    /// <summary>
    /// Full replace of the deck's character lineup from a SetTutorialProgressAndReplaceDeck request.
    /// Removes existing EntityIUserDeckCharacter records for the deck, creates fresh ones with new
    /// UUIDs, and updates EntityIUserDeck to reference the newly generated character UUIDs.
    /// </summary>
    private static void UpsertDeck(DarkUserMemoryDatabase db, SetTutorialProgressAndReplaceDeckRequest request)
    {
        DeckType deckType = (DeckType)request.DeckType;

        EntityIUserDeck? existing = db.EntityIUserDeck
            .FirstOrDefault(d => d.DeckType == deckType && d.UserDeckNumber == request.UserDeckNumber);

        if (existing is not null)
        {
            HashSet<string> oldUuids = [];
            if (!string.IsNullOrEmpty(existing.UserDeckCharacterUuid01)) { oldUuids.Add(existing.UserDeckCharacterUuid01); }
            if (!string.IsNullOrEmpty(existing.UserDeckCharacterUuid02)) { oldUuids.Add(existing.UserDeckCharacterUuid02); }
            if (!string.IsNullOrEmpty(existing.UserDeckCharacterUuid03)) { oldUuids.Add(existing.UserDeckCharacterUuid03); }
            db.EntityIUserDeckCharacter.RemoveAll(dc => oldUuids.Contains(dc.UserDeckCharacterUuid));
            db.EntityIUserDeckCharacterDressupCostume.RemoveAll(dc => oldUuids.Contains(dc.UserDeckCharacterUuid));
            db.EntityIUserDeckPartsGroup.RemoveAll(pg => oldUuids.Contains(pg.UserDeckCharacterUuid));
            db.EntityIUserDeckSubWeaponGroup.RemoveAll(swg => oldUuids.Contains(swg.UserDeckCharacterUuid));
        }

        string uuid01 = CreateDeckCharacter(db, request.Deck?.Character01);
        string uuid02 = CreateDeckCharacter(db, request.Deck?.Character02);
        string uuid03 = CreateDeckCharacter(db, request.Deck?.Character03);

        if (existing is null)
        {
            db.EntityIUserDeck.Add(new EntityIUserDeck
            {
                UserId = db.UserId,
                DeckType = deckType,
                UserDeckNumber = request.UserDeckNumber,
                UserDeckCharacterUuid01 = uuid01,
                UserDeckCharacterUuid02 = uuid02,
                UserDeckCharacterUuid03 = uuid03,
                Name = $"Loadout {request.UserDeckNumber}",
                Power = 0
            });
        }
        else
        {
            existing.UserDeckCharacterUuid01 = uuid01;
            existing.UserDeckCharacterUuid02 = uuid02;
            existing.UserDeckCharacterUuid03 = uuid03;
        }
    }

    /// <summary>Creates a deck character record from a DeckCharacter slot proto and returns the new UUID.</summary>
    private static string CreateDeckCharacter(DarkUserMemoryDatabase db, DeckCharacter? slot)
    {
        if (slot is null || string.IsNullOrEmpty(slot.UserCostumeUuid))
        {
            return "";
        }

        string newUuid = Guid.NewGuid().ToString();
        db.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter
        {
            UserId = db.UserId,
            UserDeckCharacterUuid = newUuid,
            UserCostumeUuid = slot.UserCostumeUuid,
            MainUserWeaponUuid = slot.MainUserWeaponUuid,
            UserCompanionUuid = slot.UserCompanionUuid,
            UserThoughtUuid = slot.UserThoughtUuid,
            Power = 0
        });

        if (slot.DressupCostumeId != 0)
        {
            db.EntityIUserDeckCharacterDressupCostume.Add(new EntityIUserDeckCharacterDressupCostume
            {
                UserId = db.UserId,
                UserDeckCharacterUuid = newUuid,
                DressupCostumeId = slot.DressupCostumeId
            });
        }

        for (int i = 0; i < slot.UserPartsUuid.Count; i++)
        {
            if (string.IsNullOrEmpty(slot.UserPartsUuid[i])) { continue; }
            db.EntityIUserDeckPartsGroup.Add(new EntityIUserDeckPartsGroup
            {
                UserId = db.UserId,
                UserDeckCharacterUuid = newUuid,
                UserPartsUuid = slot.UserPartsUuid[i],
                SortOrder = i + 1
            });
        }

        for (int i = 0; i < slot.SubUserWeaponUuid.Count; i++)
        {
            if (string.IsNullOrEmpty(slot.SubUserWeaponUuid[i])) { continue; }
            db.EntityIUserDeckSubWeaponGroup.Add(new EntityIUserDeckSubWeaponGroup
            {
                UserId = db.UserId,
                UserDeckCharacterUuid = newUuid,
                UserWeaponUuid = slot.SubUserWeaponUuid[i],
                SortOrder = i + 1
            });
        }

        return newUuid;
    }

    /// <summary>Gives a story pickup's items and returns them for the client's "obtained" popup.</summary>
    private List<TutorialChoiceReward> GrantPickup(DarkUserMemoryDatabase db, TutorialType tutorialType, long nowMs)
    {
        if (!SkipTicketPickups.TryGetValue(tutorialType, out int count))
        {
            return [];
        }

        int skipTicketId = gameConfig.ConsumableItemIdForQuestSkipTicket;
        PossessionHelper.Apply(db, PossessionType.CONSUMABLE_ITEM, skipTicketId, count, masterDb, nowMs);
        return [new TutorialChoiceReward { PossessionType = (int)PossessionType.CONSUMABLE_ITEM, PossessionId = skipTicketId, Count = count }];
    }

    /// <summary>
    /// Uses the items a tutorial's <c>EntityMTutorialConsumePossessionGroup</c> rows list (the client checks the user
    /// holds them before offering the step). Nothing is taken if any of them is short.
    /// </summary>
    private void ConsumeTutorialPossessions(DarkUserMemoryDatabase db, TutorialType tutorialType)
    {
        List<EntityMTutorialConsumePossessionGroup> rows = [.. masterDb.EntityMTutorialConsumePossessionGroup.Where(r => r.TutorialType == tutorialType)];
        if (rows.Count == 0 || !rows.All(r => HeldCount(db, r.PossessionType, r.PossessionId) >= r.Count))
        {
            return;
        }

        foreach (EntityMTutorialConsumePossessionGroup row in rows)
        {
            switch (row.PossessionType)
            {
                case PossessionType.CONSUMABLE_ITEM:
                    db.EntityIUserConsumableItem.First(c => c.ConsumableItemId == row.PossessionId).Count -= row.Count;
                    break;
                case PossessionType.MATERIAL:
                    db.EntityIUserMaterial.First(m => m.MaterialId == row.PossessionId).Count -= row.Count;
                    break;
            }
        }
    }

    /// <summary>How many of a stackable item the user holds; 0 for kinds a tutorial never consumes.</summary>
    private static int HeldCount(DarkUserMemoryDatabase db, PossessionType type, int id) => type switch
    {
        PossessionType.CONSUMABLE_ITEM => db.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == id)?.Count ?? 0,
        PossessionType.MATERIAL => db.EntityIUserMaterial.FirstOrDefault(m => m.MaterialId == id)?.Count ?? 0,
        _ => 0
    };
}

