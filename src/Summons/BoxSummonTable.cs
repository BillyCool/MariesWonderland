using System.Text.Json;
using System.Text.Json.Serialization;
using MariesWonderland.Data;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Summons;

/// <summary>The two kinds of box summon: chapter summons (featured prizes per player) and Mama's Dream (limited prizes shared by everyone).</summary>
public enum BoxSummonKind
{
    CHAPTER,
    MAMAS_DREAM
}

/// <summary>
/// A prize of a box summon. Every prize has a <see cref="Weight"/> (its chance against the others' weights); limited
/// (featured) prizes also have a <see cref="Stock"/> and stop coming up once it runs out.
/// </summary>
public sealed record BoxSummonPrize(PossessionType PossessionType, int PossessionId, int Count, int Weight, int Stock = 0);

/// <summary>
/// A box summon's prizes. Each draw picks by weight among the unlimited prizes and the limited ones still in stock;
/// featured prizes are rarer by having smaller weights. Once the limited prizes run out only unlimited ones come up.
/// </summary>
public sealed record BoxSummonPool(string PoolId, IReadOnlyList<BoxSummonPrize> Limited, IReadOnlyList<BoxSummonPrize> Unlimited);

/// <summary>
/// A box summon: one ticket per draw, one prize per ticket. Chapter summons are listed once the player has reached
/// <see cref="MainQuestChapterId"/> (Dark Memories has none) and refill their featured prizes every month.
/// </summary>
public sealed record BoxSummon(int GachaId, BoxSummonKind Kind, string AssetName, int TicketItemId, int MainQuestChapterId, BoxSummonPool Pool);

/// <summary>
/// The box summons (<c>Data/Custom/BoxSummons.json</c>): chapter summons and Mama's Dream summons, which the client
/// never had the data for. The chapter pools are a stand-in until the real ones are known.
/// </summary>
public sealed class BoxSummonTable
{
    public static readonly string DefaultPath = Path.Combine(AppContext.BaseDirectory, "Data", "Custom", "BoxSummons.json");

    public static readonly BoxSummonTable Empty = new([]);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly Dictionary<int, BoxSummon> _byGachaId;

    public BoxSummonTable(IEnumerable<BoxSummon> summons)
    {
        _byGachaId = summons.ToDictionary(s => s.GachaId);
    }

    public IReadOnlyCollection<BoxSummon> All => _byGachaId.Values;

    /// <summary>The box summon with this gacha id; null for any other summon.</summary>
    public BoxSummon? Find(int gachaId) => _byGachaId.GetValueOrDefault(gachaId);

    /// <summary>
    /// True once the player has reached the summon's main chapter (played any of its quests); Dark Memories, which
    /// has no chapter, once any Dark Memory quest has been played.
    /// </summary>
    public static bool IsChapterReached(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, BoxSummon summon)
    {
        HashSet<int> questIds;
        if (summon.MainQuestChapterId > 0)
        {
            int? groupId = masterDb.EntityMMainQuestChapter.FirstOrDefault(c => c.MainQuestChapterId == summon.MainQuestChapterId)?.MainQuestSequenceGroupId;
            HashSet<int> sequenceIds = [.. masterDb.EntityMMainQuestSequenceGroup.Where(g => g.MainQuestSequenceGroupId == groupId).Select(g => g.MainQuestSequenceId)];
            questIds = [.. masterDb.EntityMMainQuestSequence.Where(s => sequenceIds.Contains(s.MainQuestSequenceId)).Select(s => s.QuestId)];
        }
        else
        {
            HashSet<int> groupIds = [.. masterDb.EntityMEventQuestChapter.Where(c => c.EventQuestType == EventQuestType.END_CONTENTS).Select(c => c.EventQuestSequenceGroupId)];
            HashSet<int> sequenceIds = [.. masterDb.EntityMEventQuestSequenceGroup.Where(g => groupIds.Contains(g.EventQuestSequenceGroupId)).Select(g => g.EventQuestSequenceId)];
            questIds = [.. masterDb.EntityMEventQuestSequence.Where(s => sequenceIds.Contains(s.EventQuestSequenceId)).Select(s => s.QuestId)];
        }

        return userDb.EntityIUserQuest.Any(q => questIds.Contains(q.QuestId));
    }

    public static BoxSummonTable Load(string path)
    {
        SummonFile file = JsonSerializer.Deserialize<SummonFile>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException($"'{path}' is empty.");

        Dictionary<string, BoxSummonPool> pools = [];
        foreach (PoolEntry pool in file.Pools)
        {
            List<BoxSummonPrize> limited = [.. pool.Limited.Select(p => p.ToPrize())];
            List<BoxSummonPrize> unlimited = [.. pool.Unlimited.Select(p => p.ToPrize())];
            if (limited.Any(p => p.Stock <= 0) || limited.Concat(unlimited).Any(p => p.Weight <= 0) || unlimited.Count == 0)
            {
                throw new InvalidDataException($"Box summon pool '{pool.PoolId}' in '{path}' needs a weight on every prize, a stock on every limited prize, and at least one unlimited prize.");
            }

            if (!pools.TryAdd(pool.PoolId, new BoxSummonPool(pool.PoolId, limited, unlimited)))
            {
                throw new InvalidDataException($"Box summon pool '{pool.PoolId}' is listed twice in '{path}'.");
            }
        }

        List<BoxSummon> summons = [];
        foreach (SummonEntry summon in file.Summons)
        {
            if (!pools.TryGetValue(summon.PoolId, out BoxSummonPool? pool))
            {
                throw new InvalidDataException($"Box summon {summon.GachaId} in '{path}' names an unknown pool '{summon.PoolId}'.");
            }

            summons.Add(new BoxSummon(summon.GachaId, summon.Kind, summon.AssetName, summon.TicketItemId, summon.MainQuestChapterId ?? 0, pool));
        }

        if (summons.Select(s => s.GachaId).Distinct().Count() != summons.Count)
        {
            throw new InvalidDataException($"A box summon gacha id is listed twice in '{path}'.");
        }

        return new BoxSummonTable(summons);
    }

    private sealed record SummonFile(
        [property: JsonPropertyName("_about")] string? About,
        List<PoolEntry> Pools,
        List<SummonEntry> Summons);

    private sealed record PoolEntry(string PoolId, string? Note, List<PrizeEntry> Limited, List<PrizeEntry> Unlimited);

    private sealed record PrizeEntry(PossessionType PossessionType, int PossessionId, int Count, int Weight, int? Stock)
    {
        public BoxSummonPrize ToPrize() => new(PossessionType, PossessionId, Count, Weight, Stock ?? 0);
    }

    private sealed record SummonEntry(int GachaId, BoxSummonKind Kind, string AssetName, int TicketItemId, int? MainQuestChapterId, string PoolId, string? Note);
}
