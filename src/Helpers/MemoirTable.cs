using System.Text.Json;
using System.Text.Json.Serialization;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>One kind of sub-status roll of a rarity: a value from <see cref="MinValue"/> to <see cref="MaxValue"/> in steps of <see cref="Step"/>.</summary>
public sealed record MemoirSubStatusRoll(int PartsStatusSubLotteryId, StatusKindType StatusKindType, StatusCalculationType StatusCalculationType, int MinValue, int MaxValue, int Step)
{
    public int RollValue(Random random) => MinValue + Step * random.Next((MaxValue - MinValue) / Step + 1);
}

/// <summary>
/// The server-side memoir tables master data lacks (<c>Data/Custom/MemoirSubStatusRolls.json</c> and
/// <c>MemoirDrops.json</c>): which sub-status values a memoir of each rarity rolls, which grade (the game's "Sub No.")
/// a dropped memoir has, and how many sub-status slots a grade starts with. The administrator accepted the numbers as
/// they are; the files note where each came from.
/// </summary>
public sealed class MemoirTable
{
    public static readonly string RollsPath = Path.Combine(AppContext.BaseDirectory, "Data", "Custom", "MemoirSubStatusRolls.json");
    public static readonly string DropsPath = Path.Combine(AppContext.BaseDirectory, "Data", "Custom", "MemoirDrops.json");

    private static readonly Lazy<MemoirTable> DefaultTable = new(() => Load(RollsPath, DropsPath));

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly Dictionary<RarityType, IReadOnlyList<MemoirSubStatusRoll>> _rolls;
    private readonly Dictionary<RarityType, IReadOnlyList<(int Grade, int Weight)>> _gradeOdds;
    private readonly Dictionary<int, IReadOnlyList<int>> _startingSlots;

    public MemoirTable(
        Dictionary<RarityType, IReadOnlyList<MemoirSubStatusRoll>> rolls,
        Dictionary<RarityType, IReadOnlyList<(int Grade, int Weight)>> gradeOdds,
        Dictionary<int, IReadOnlyList<int>> startingSlots,
        int maxSlots,
        int upgradeEveryLevels)
    {
        _rolls = rolls;
        _gradeOdds = gradeOdds;
        _startingSlots = startingSlots;
        MaxSlots = maxSlots;
        UpgradeEveryLevels = upgradeEveryLevels;
    }

    /// <summary>The tables loaded from the files next to the server (loaded on first use).</summary>
    public static MemoirTable Default => DefaultTable.Value;

    /// <summary>Sub-status slots a memoir can have.</summary>
    public int MaxSlots { get; }

    /// <summary>A memoir fills an empty slot, or upgrades one, every this many levels.</summary>
    public int UpgradeEveryLevels { get; }

    public static MemoirTable Load(string rollsPath, string dropsPath)
    {
        RollsFile rolls = JsonSerializer.Deserialize<RollsFile>(File.ReadAllText(rollsPath), JsonOptions)
            ?? throw new InvalidDataException($"{rollsPath} is empty.");
        DropsFile drops = JsonSerializer.Deserialize<DropsFile>(File.ReadAllText(dropsPath), JsonOptions)
            ?? throw new InvalidDataException($"{dropsPath} is empty.");

        return new MemoirTable(
            rolls.Rarities.ToDictionary(
                r => r.RarityType,
                r => (IReadOnlyList<MemoirSubStatusRoll>)[.. r.Rolls.Select(x => new MemoirSubStatusRoll(x.PartsStatusSubLotteryId, x.StatusKindType, x.StatusCalculationType, x.MinValue, x.MaxValue, x.Step))]),
            drops.GradeOdds.ToDictionary(
                r => r.RarityType,
                r => (IReadOnlyList<(int Grade, int Weight)>)[.. r.Grades.Select(g => (g.Grade, g.Weight))]),
            drops.StartingSlots.ToDictionary(s => s.Grade, s => (IReadOnlyList<int>)s.Slots),
            drops.MaxSlots,
            drops.UpgradeEveryLevels);
    }

    /// <summary>The grade a dropped memoir of this rarity has, by the rarity's odds; 0 when the rarity has none.</summary>
    public int RollGrade(RarityType rarity, Random random)
    {
        if (!_gradeOdds.TryGetValue(rarity, out IReadOnlyList<(int Grade, int Weight)>? odds) || odds.Count == 0)
        {
            return 0;
        }

        int pick = random.Next(odds.Sum(o => o.Weight));
        foreach ((int grade, int weight) in odds)
        {
            pick -= weight;
            if (pick < 0)
            {
                return grade;
            }
        }

        return odds[^1].Grade;
    }

    /// <summary>The sub-status slots a memoir of this grade starts with: one of the grade's counts, each equally likely.</summary>
    public int RollStartingSlots(int grade, Random random)
    {
        if (!_startingSlots.TryGetValue(grade, out IReadOnlyList<int>? slots) || slots.Count == 0)
        {
            return 0;
        }

        return Math.Min(slots[random.Next(slots.Count)], MaxSlots);
    }

    /// <summary>
    /// A new sub-status for a memoir of this rarity: a kind not in <paramref name="usedKinds"/> (a memoir never has the
    /// same kind twice), then flat or percent where the kind has both, each equally likely. Null when the rarity has no
    /// rolls or no kind is left.
    /// </summary>
    public MemoirSubStatusRoll? PickNewRoll(RarityType rarity, ICollection<StatusKindType> usedKinds, Random random)
    {
        if (!_rolls.TryGetValue(rarity, out IReadOnlyList<MemoirSubStatusRoll>? rolls))
        {
            return null;
        }

        List<StatusKindType> kinds = [.. rolls.Select(r => r.StatusKindType).Distinct().Where(k => !usedKinds.Contains(k))];
        if (kinds.Count == 0)
        {
            return null;
        }

        StatusKindType kind = kinds[random.Next(kinds.Count)];
        List<MemoirSubStatusRoll> variants = [.. rolls.Where(r => r.StatusKindType == kind)];
        return variants[random.Next(variants.Count)];
    }

    /// <summary>The roll an existing sub-status of this rarity upgrades by, found by its lottery id.</summary>
    public MemoirSubStatusRoll? FindRoll(RarityType rarity, int partsStatusSubLotteryId)
        => _rolls.TryGetValue(rarity, out IReadOnlyList<MemoirSubStatusRoll>? rolls)
            ? rolls.FirstOrDefault(r => r.PartsStatusSubLotteryId == partsStatusSubLotteryId)
            : null;

    private sealed class RollsFile
    {
        public List<RarityRolls> Rarities { get; set; } = [];
    }

    private sealed class RarityRolls
    {
        public RarityType RarityType { get; set; }

        public List<RollRow> Rolls { get; set; } = [];
    }

    private sealed class RollRow
    {
        public int PartsStatusSubLotteryId { get; set; }

        public StatusKindType StatusKindType { get; set; }

        public StatusCalculationType StatusCalculationType { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public int Step { get; set; }
    }

    private sealed class DropsFile
    {
        public int MaxSlots { get; set; }

        public int UpgradeEveryLevels { get; set; }

        public List<RarityGrades> GradeOdds { get; set; } = [];

        public List<GradeSlots> StartingSlots { get; set; } = [];
    }

    private sealed class RarityGrades
    {
        public RarityType RarityType { get; set; }

        public List<GradeWeight> Grades { get; set; } = [];
    }

    private sealed class GradeWeight
    {
        public int Grade { get; set; }

        public int Weight { get; set; }
    }

    private sealed class GradeSlots
    {
        public int Grade { get; set; }

        public List<int> Slots { get; set; } = [];
    }
}
