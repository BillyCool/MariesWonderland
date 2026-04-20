using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Deck;
using MariesWonderland.Proto.Tutorial;

namespace MariesWonderland.Services;

public class TutorialService(UserDataStore store, DarkMasterMemoryDatabase masterDb) : MariesWonderland.Proto.Tutorial.TutorialService.TutorialServiceBase
{
    /// <summary>Advances tutorial progress, triggers starter deck creation on menu tutorials, and returns tutorial rewards.</summary>
    public override Task<SetTutorialProgressResponse> SetTutorialProgress(SetTutorialProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        UpsertTutorialProgress(userDb, userId, (TutorialType)request.TutorialType, request.ProgressPhase, request.ChoiceId);

        if (request.TutorialType == (int)TutorialType.MENU_FIRST ||
            request.TutorialType == (int)TutorialType.MENU_SECOND)
        {
            CreateStarterDeck(userDb, userId);
        }

        List<TutorialChoiceReward> rewards = ApplyTutorialRewards(userDb, userId, (TutorialType)request.TutorialType);

        SetTutorialProgressResponse response = new();
        response.TutorialChoiceReward.AddRange(rewards);
        return Task.FromResult(response);
    }

    /// <summary>Advances tutorial progress and replaces the user's deck in one operation.</summary>
    public override Task<SetTutorialProgressAndReplaceDeckResponse> SetTutorialProgressAndReplaceDeck(SetTutorialProgressAndReplaceDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(userId);

        UpsertTutorialProgress(userDb, userId, (TutorialType)request.TutorialType, request.ProgressPhase, choiceId: 0);
        UpsertDeck(userDb, userId, request);

        return Task.FromResult(new SetTutorialProgressAndReplaceDeckResponse());
    }

    /// <summary>Creates or updates a tutorial progress record, advancing to the specified phase.</summary>
    private static void UpsertTutorialProgress(DarkUserMemoryDatabase db, long userId, TutorialType type, int phase, int choiceId)
    {
        EntityIUserTutorialProgress? existing = db.EntityIUserTutorialProgress
            .FirstOrDefault(t => t.TutorialType == type);

        if (existing is null)
        {
            db.EntityIUserTutorialProgress.Add(new EntityIUserTutorialProgress
            {
                UserId = userId,
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
    /// Creates a minimal starter deck (DeckType=QUEST, DeckNumber=1) using the player's first
    /// owned costume/weapon/companion. Only runs when no deck 1 character slot exists yet.
    /// Idempotent: safe to call multiple times.
    /// </summary>
    private static void CreateStarterDeck(DarkUserMemoryDatabase db, long userId)
    {
        if (db.EntityIUserCostume.Count == 0)
        {
            return;
        }

        EntityIUserDeck? existingDeck = db.EntityIUserDeck
            .FirstOrDefault(d => d.DeckType == DeckType.QUEST && d.UserDeckNumber == 1);

        if (existingDeck is not null && !string.IsNullOrEmpty(existingDeck.UserDeckCharacterUuid01))
        {
            return;
        }

        // Hardcoded to Rion & Everlasting Cardia
        string costumeUuid = db.EntityIUserCostume
            .Where(c => c.CostumeId == Constants.StartingDeckCostumeId)
            .Select(c => c.UserCostumeUuid)
            .Single();

        string weaponUuid = db.EntityIUserWeapon
            .Where(w => w.WeaponId == Constants.StartingDeckWeaponId)
            .Select(w => w.UserWeaponUuid)
            .Single();

        string dcUuid = Guid.NewGuid().ToString();

        db.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter
        {
            UserId = userId,
            UserDeckCharacterUuid = dcUuid,
            UserCostumeUuid = costumeUuid,
            MainUserWeaponUuid = weaponUuid,
            Power = 0
        });

        if (existingDeck is null)
        {
            db.EntityIUserDeck.Add(new EntityIUserDeck
            {
                UserId = userId,
                DeckType = DeckType.QUEST,
                UserDeckNumber = 1,
                UserDeckCharacterUuid01 = dcUuid,
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
                UserId = userId,
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
    private static void UpsertDeck(DarkUserMemoryDatabase db, long userId, SetTutorialProgressAndReplaceDeckRequest request)
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

        string uuid01 = CreateDeckCharacter(db, userId, request.Deck?.Character01);
        string uuid02 = CreateDeckCharacter(db, userId, request.Deck?.Character02);
        string uuid03 = CreateDeckCharacter(db, userId, request.Deck?.Character03);

        if (existing is null)
        {
            db.EntityIUserDeck.Add(new EntityIUserDeck
            {
                UserId = userId,
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
    private static string CreateDeckCharacter(DarkUserMemoryDatabase db, long userId, DeckCharacter? slot)
    {
        if (slot is null || string.IsNullOrEmpty(slot.UserCostumeUuid))
        {
            return "";
        }

        string newUuid = Guid.NewGuid().ToString();
        db.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter
        {
            UserId = userId,
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
                UserId = userId,
                UserDeckCharacterUuid = newUuid,
                DressupCostumeId = slot.DressupCostumeId
            });
        }

        for (int i = 0; i < slot.UserPartsUuid.Count; i++)
        {
            if (string.IsNullOrEmpty(slot.UserPartsUuid[i])) { continue; }
            db.EntityIUserDeckPartsGroup.Add(new EntityIUserDeckPartsGroup
            {
                UserId = userId,
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
                UserId = userId,
                UserDeckCharacterUuid = newUuid,
                UserWeaponUuid = slot.SubUserWeaponUuid[i],
                SortOrder = i + 1
            });
        }

        return newUuid;
    }

    /// <summary>
    /// Grants tutorial rewards from master data keyed by tutorial type and returns the reward list
    /// for the response. Each tutorial type may grant companions, gems, items, or other possessions.
    /// </summary>
    private List<TutorialChoiceReward> ApplyTutorialRewards(DarkUserMemoryDatabase db, long userId, TutorialType tutorialType)
    {
        List<EntityMTutorialConsumePossessionGroup> rewardRows = [.. masterDb.EntityMTutorialConsumePossessionGroup
            .Where(r => r.TutorialType == tutorialType)];

        List<TutorialChoiceReward> result = [];

        foreach (EntityMTutorialConsumePossessionGroup row in rewardRows)
        {
            PossessionHelper.Apply(db, userId, row.PossessionType, row.PossessionId, row.Count, masterDb);

            result.Add(new TutorialChoiceReward
            {
                PossessionType = (int)row.PossessionType,
                PossessionId = row.PossessionId,
                Count = row.Count
            });
        }

        return result;
    }
}

