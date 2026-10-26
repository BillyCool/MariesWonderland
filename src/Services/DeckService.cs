using Grpc.Core;
using MariesWonderland.Arena;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Deck;

namespace MariesWonderland.Services;

public class DeckService(UserDataStore store, DarkMasterMemoryDatabase masterDb, TimeProvider timeProvider) : MariesWonderland.Proto.Deck.DeckService.DeckServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Renames a deck.</summary>
    public override Task<UpdateNameResponse> UpdateName(UpdateNameRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d =>
            d.DeckType == (DeckType)request.DeckType &&
            d.UserDeckNumber == request.UserDeckNumber);

        deck?.Name = request.Name;

        return Task.FromResult(new UpdateNameResponse());
    }

    /// <summary>Replaces a deck's character lineup with new entries.</summary>
    public override Task<ReplaceDeckResponse> ReplaceDeck(ReplaceDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        if (request.Deck != null)
        {
            ApplyDeckReplacement(userDb, (DeckType)request.DeckType, request.UserDeckNumber, request.Deck);
        }

        return Task.FromResult(new ReplaceDeckResponse());
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

    /// <summary>
    /// Chooses which Arena (PvP) deck defends. The client sends only the deck number; others see the new deck on their
    /// opponent cards straight away.
    /// </summary>
    public override Task<SetPvpDefenseDeckResponse> SetPvpDefenseDeck(SetPvpDefenseDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        if (FindDeck(userDb, DeckType.PVP, request.UserDeckNumber) == null)
        {
            return Task.FromResult(new SetPvpDefenseDeckResponse());
        }

        EntityIUserPvpDefenseDeck defense = userDb.EntityIUserPvpDefenseDeck.GetOrCreate(userId);
        defense.UserDeckNumber = request.UserDeckNumber;
        RefreshArenaDefense(userDb, DeckType.PVP, request.UserDeckNumber);
        return Task.FromResult(new SetPvpDefenseDeckResponse());
    }

    /// <summary>When the deck that changed is the Arena defense deck, refreshes what other players see of it.</summary>
    private void RefreshArenaDefense(DarkUserMemoryDatabase userDb, DeckType deckType, int deckNumber)
    {
        if (deckType != DeckType.PVP || deckNumber != ArenaDecks.DefenseDeckNumber(userDb))
        {
            return;
        }

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        EntityMPvpSeason? season = ArenaHelper.CurrentSeason(_masterDb, nowMs);
        if (season != null)
        {
            _store.Arena.UpdateDefense(season.PvpSeasonId, userDb.UserId, ArenaDecks.Summary(userDb, _masterDb, deckType, deckNumber), nowMs);
        }
    }

    /// <summary>Copies a deck's character lineup into another deck, recreating linked slot records.</summary>
    public override Task<CopyDeckResponse> CopyDeck(CopyDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserDeck? sourceDeck = FindDeck(userDb, (DeckType)request.FromDeckType, request.FromUserDeckNumber);
        if (sourceDeck == null)
        {
            return Task.FromResult(new CopyDeckResponse());
        }

        Deck sourceSlots = ReadDeck(userDb, sourceDeck);
        ApplyDeckReplacement(userDb, (DeckType)request.ToDeckType, request.ToUserDeckNumber, sourceSlots);

        EntityIUserDeck? targetDeck = FindDeck(userDb, (DeckType)request.ToDeckType, request.ToUserDeckNumber);
        if (targetDeck != null)
        {
            targetDeck.Name = sourceDeck.Name;
        }

        return Task.FromResult(new CopyDeckResponse { ResultType = 1 });
    }

    /// <summary>Removes a deck and its linked character records.</summary>
    public override Task<RemoveDeckResponse> RemoveDeck(RemoveDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserDeck? deck = FindDeck(userDb, (DeckType)request.DeckType, request.UserDeckNumber);
        if (deck != null)
        {
            RemoveDeckLinkedRecords(userDb, deck);
            userDb.EntityIUserDeck.Remove(deck);
        }

        return Task.FromResult(new RemoveDeckResponse());
    }

    /// <summary>Updates deck and character power values from the client.</summary>
    public override Task<RefreshDeckPowerResponse> RefreshDeckPower(RefreshDeckPowerRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        if (request.DeckPower != null)
        {
            ApplyDeckPowerRefresh(userDb, (DeckType)request.DeckType, request.UserDeckNumber, request.DeckPower);
            RefreshArenaDefense(userDb, (DeckType)request.DeckType, request.UserDeckNumber);
        }

        return Task.FromResult(new RefreshDeckPowerResponse());
    }

    /// <summary>Stub for triple deck name update; returns empty response.</summary>
    public override Task<UpdateTripleDeckNameResponse> UpdateTripleDeckName(UpdateTripleDeckNameRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserTripleDeck tripleDeck = GetOrCreateTripleDeck(userDb, (DeckType)request.DeckType, request.UserDeckNumber);
        tripleDeck.Name = request.Name;

        return Task.FromResult(new UpdateTripleDeckNameResponse());
    }

    /// <summary>Replaces up to three decks' character lineups in one operation.</summary>
    public override Task<ReplaceTripleDeckResponse> ReplaceTripleDeck(ReplaceTripleDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (DeckDetail? detail in new[] { request.DeckDetail01, request.DeckDetail02, request.DeckDetail03 })
        {
            if (detail?.Deck == null)
            {
                continue;
            }

            ApplyDeckReplacement(userDb, (DeckType)detail.DeckType, detail.UserDeckNumber, detail.Deck);
        }

        EntityIUserTripleDeck tripleDeck = GetOrCreateTripleDeck(userDb, (DeckType)request.DeckType, request.UserDeckNumber);
        tripleDeck.DeckNumber01 = request.DeckDetail01?.UserDeckNumber ?? 0;
        tripleDeck.DeckNumber02 = request.DeckDetail02?.UserDeckNumber ?? 0;
        tripleDeck.DeckNumber03 = request.DeckDetail03?.UserDeckNumber ?? 0;

        return Task.FromResult(new ReplaceTripleDeckResponse());
    }

    /// <summary>Replaces multiple decks' character lineups in one operation.</summary>
    public override Task<ReplaceMultiDeckResponse> ReplaceMultiDeck(ReplaceMultiDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (DeckDetail detail in request.DeckDetail)
        {
            if (detail?.Deck == null)
            {
                continue;
            }

            ApplyDeckReplacement(userDb, (DeckType)detail.DeckType, detail.UserDeckNumber, detail.Deck);
        }

        return Task.FromResult(new ReplaceMultiDeckResponse());
    }

    /// <summary>Updates power values for multiple decks in one operation.</summary>
    public override Task<RefreshMultiDeckPowerResponse> RefreshMultiDeckPower(RefreshMultiDeckPowerRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (DeckPowerInfo info in request.DeckPowerInfo)
        {
            if (info.DeckPower == null)
            {
                continue;
            }

            ApplyDeckPowerRefresh(userDb, (DeckType)info.DeckType, info.UserDeckNumber, info.DeckPower);
            RefreshArenaDefense(userDb, (DeckType)info.DeckType, info.UserDeckNumber);
        }

        return Task.FromResult(new RefreshMultiDeckPowerResponse());
    }

    /// <summary>Removes old deck characters and creates new ones from the provided deck proto.</summary>
    private static void ApplyDeckReplacement(DarkUserMemoryDatabase userDb, DeckType deckType, int deckNumber, Deck deck)
    {
        EntityIUserDeck? existing = FindDeck(userDb, deckType, deckNumber);

        if (existing != null)
        {
            RemoveDeckLinkedRecords(userDb, existing);
        }

        string uuid01 = CreateDeckCharacter(userDb, deck.Character01);
        string uuid02 = CreateDeckCharacter(userDb, deck.Character02);
        string uuid03 = CreateDeckCharacter(userDb, deck.Character03);

        if (existing == null)
        {
            existing = new EntityIUserDeck
            {
                UserId = userDb.UserId,
                DeckType = deckType,
                UserDeckNumber = deckNumber,
                Name = $"Loadout {deckNumber}",
                Power = 0
            };
            userDb.EntityIUserDeck.Add(existing);
        }

        existing.UserDeckCharacterUuid01 = uuid01;
        existing.UserDeckCharacterUuid02 = uuid02;
        existing.UserDeckCharacterUuid03 = uuid03;
    }

    private static EntityIUserDeck? FindDeck(DarkUserMemoryDatabase userDb, DeckType deckType, int deckNumber) =>
        userDb.EntityIUserDeck.FirstOrDefault(d => d.DeckType == deckType && d.UserDeckNumber == deckNumber);

    private static EntityIUserTripleDeck GetOrCreateTripleDeck(DarkUserMemoryDatabase userDb, DeckType deckType, int deckNumber)
    {
        EntityIUserTripleDeck? existing = userDb.EntityIUserTripleDeck
            .FirstOrDefault(td => td.DeckType == deckType && td.UserDeckNumber == deckNumber);
        if (existing is not null)
        {
            return existing;
        }

        EntityIUserTripleDeck tripleDeck = new()
        {
            UserId = userDb.UserId,
            DeckType = deckType,
            UserDeckNumber = deckNumber,
            Name = string.Empty
        };
        userDb.EntityIUserTripleDeck.Add(tripleDeck);
        return tripleDeck;
    }

    private static void RemoveDeckLinkedRecords(DarkUserMemoryDatabase userDb, EntityIUserDeck deck)
    {
        HashSet<string> oldUuids =
        [
            .. GetDeckCharacterUuids(deck).Where(uuid => !string.IsNullOrEmpty(uuid))
        ];

        if (oldUuids.Count == 0)
        {
            return;
        }

        userDb.EntityIUserDeckCharacter.RemoveAll(dc => oldUuids.Contains(dc.UserDeckCharacterUuid));
        userDb.EntityIUserDeckCharacterDressupCostume.RemoveAll(dc => oldUuids.Contains(dc.UserDeckCharacterUuid));
        userDb.EntityIUserDeckPartsGroup.RemoveAll(pg => oldUuids.Contains(pg.UserDeckCharacterUuid));
        userDb.EntityIUserDeckSubWeaponGroup.RemoveAll(swg => oldUuids.Contains(swg.UserDeckCharacterUuid));
    }

    private static IEnumerable<string> GetDeckCharacterUuids(EntityIUserDeck deck)
    {
        yield return deck.UserDeckCharacterUuid01;
        yield return deck.UserDeckCharacterUuid02;
        yield return deck.UserDeckCharacterUuid03;
    }

    private static Deck ReadDeck(DarkUserMemoryDatabase userDb, EntityIUserDeck deck) =>
        new()
        {
            Character01 = ReadDeckCharacter(userDb, deck.UserDeckCharacterUuid01),
            Character02 = ReadDeckCharacter(userDb, deck.UserDeckCharacterUuid02),
            Character03 = ReadDeckCharacter(userDb, deck.UserDeckCharacterUuid03)
        };

    private static DeckCharacter? ReadDeckCharacter(DarkUserMemoryDatabase userDb, string userDeckCharacterUuid)
    {
        if (string.IsNullOrEmpty(userDeckCharacterUuid))
        {
            return null;
        }

        EntityIUserDeckCharacter? deckCharacter = userDb.EntityIUserDeckCharacter
            .FirstOrDefault(dc => dc.UserDeckCharacterUuid == userDeckCharacterUuid);

        if (deckCharacter == null)
        {
            return null;
        }

        DeckCharacter slot = new()
        {
            UserCostumeUuid = deckCharacter.UserCostumeUuid,
            MainUserWeaponUuid = deckCharacter.MainUserWeaponUuid,
            UserCompanionUuid = deckCharacter.UserCompanionUuid,
            UserThoughtUuid = deckCharacter.UserThoughtUuid
        };

        EntityIUserDeckCharacterDressupCostume? dressup = userDb.EntityIUserDeckCharacterDressupCostume
            .FirstOrDefault(dc => dc.UserDeckCharacterUuid == userDeckCharacterUuid);
        if (dressup != null)
        {
            slot.DressupCostumeId = dressup.DressupCostumeId;
        }

        slot.UserPartsUuid.AddRange(userDb.EntityIUserDeckPartsGroup
            .Where(pg => pg.UserDeckCharacterUuid == userDeckCharacterUuid)
            .OrderBy(pg => pg.SortOrder)
            .Select(pg => pg.UserPartsUuid));
        slot.SubUserWeaponUuid.AddRange(userDb.EntityIUserDeckSubWeaponGroup
            .Where(swg => swg.UserDeckCharacterUuid == userDeckCharacterUuid)
            .OrderBy(swg => swg.SortOrder)
            .Select(swg => swg.UserWeaponUuid));

        return slot;
    }

    /// <summary>Updates deck and character power values and tracks max deck power per type.</summary>
    private static void ApplyDeckPowerRefresh(DarkUserMemoryDatabase userDb, DeckType deckType, int deckNumber, DeckPower deckPower)
    {
        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d =>
            d.DeckType == deckType && d.UserDeckNumber == deckNumber);

        if (deck != null)
        {
            deck.Power = deckPower.Power;
        }

        DeckCharacterPower?[] charPowers =
        [
            deckPower.DeckCharacterPower01,
            deckPower.DeckCharacterPower02,
            deckPower.DeckCharacterPower03,
        ];

        foreach (DeckCharacterPower? cp in charPowers)
        {
            if (cp == null || string.IsNullOrEmpty(cp.UserDeckCharacterUuid))
            {
                continue;
            }

            EntityIUserDeckCharacter? dc = userDb.EntityIUserDeckCharacter.FirstOrDefault(c =>
                c.UserDeckCharacterUuid == cp.UserDeckCharacterUuid);

            if (dc != null)
            {
                dc.Power = cp.Power;
            }
        }

        EntityIUserDeckTypeNote? note = userDb.EntityIUserDeckTypeNote.FirstOrDefault(n =>
            n.DeckType == deckType);

        if (note == null)
        {
            note = new EntityIUserDeckTypeNote { UserId = userDb.UserId, DeckType = deckType };
            userDb.EntityIUserDeckTypeNote.Add(note);
        }

        if (deckPower.Power > note.MaxDeckPower)
        {
            note.MaxDeckPower = deckPower.Power;
        }
    }
}
