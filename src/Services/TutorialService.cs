using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Tutorial;

namespace MariesWonderland.Services;

public class TutorialService(UserDataStore store) : MariesWonderland.Proto.Tutorial.TutorialService.TutorialServiceBase
{
    public override Task<SetTutorialProgressResponse> SetTutorialProgress(SetTutorialProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        UpsertTutorialProgress(userDb, userId, (TutorialType)request.TutorialType, request.ProgressPhase, request.ChoiceId);

        SetTutorialProgressResponse response = new();
        foreach ((string k, Proto.Data.DiffData v) in UserDataDiffBuilder.Delta(before, userDb))
            response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    public override Task<SetTutorialProgressAndReplaceDeckResponse> SetTutorialProgressAndReplaceDeck(SetTutorialProgressAndReplaceDeckRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        UpsertTutorialProgress(userDb, userId, (TutorialType)request.TutorialType, request.ProgressPhase, choiceId: 0);
        UpsertDeck(userDb, userId, request);

        SetTutorialProgressAndReplaceDeckResponse response = new();
        foreach ((string k, Proto.Data.DiffData v) in UserDataDiffBuilder.Delta(before, userDb))
            response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    private static void UpsertTutorialProgress(DarkUserMemoryDatabase db, long userId, TutorialType type, int phase, int choiceId)
    {
        EntityIUserTutorialProgress? existing = db.EntityIUserTutorialProgress
            .FirstOrDefault(t => t.UserId == userId && t.TutorialType == type);

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
            existing.ProgressPhase = phase;
            existing.ChoiceId = choiceId;
        }
    }

    /// <summary>
    /// Upserts the user's deck from a SetTutorialProgressAndReplaceDeck request.
    /// During tutorial the client sends the deck it wants to play with — we record the
    /// costume UUIDs from each slot and seed sensible defaults for name and power.
    /// </summary>
    private static void UpsertDeck(DarkUserMemoryDatabase db, long userId, SetTutorialProgressAndReplaceDeckRequest request)
    {
        DeckType deckType = (DeckType)request.DeckType;
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string uuid01 = request.Deck?.Character01?.UserCostumeUuid ?? "";
        string uuid02 = request.Deck?.Character02?.UserCostumeUuid ?? "";
        string uuid03 = request.Deck?.Character03?.UserCostumeUuid ?? "";

        EntityIUserDeck? existing = db.EntityIUserDeck
            .FirstOrDefault(d => d.UserId == userId && d.DeckType == deckType && d.UserDeckNumber == request.UserDeckNumber);

        if (existing is null)
        {
            db.EntityIUserDeck.Add(new EntityIUserDeck(
                UserId: userId,
                DeckType: deckType,
                UserDeckNumber: request.UserDeckNumber,
                UserDeckCharacterUuid01: uuid01,
                UserDeckCharacterUuid02: uuid02,
                UserDeckCharacterUuid03: uuid03,
                Name: "Deck 1",
                Power: 100,
                LatestVersion: now
            ));
        }
        else
        {
            if (!string.IsNullOrEmpty(uuid01)) existing.UserDeckCharacterUuid01 = uuid01;
            if (!string.IsNullOrEmpty(uuid02)) existing.UserDeckCharacterUuid02 = uuid02;
            if (!string.IsNullOrEmpty(uuid03)) existing.UserDeckCharacterUuid03 = uuid03;
            existing.LatestVersion = now;
        }
    }
}

