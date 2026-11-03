using System.Text.Json;

namespace MariesWonderland.Helpers;

/// <summary>
/// The starter companion each answer of Mama's companion question gives (<c>Data/Custom/TutorialCompanions.json</c>):
/// a server-side table master data lacks, since the choice only exists in the story script.
/// </summary>
public sealed class StarterCompanionTable(IReadOnlyDictionary<int, int> companionByChoice)
{
    public static readonly string DefaultPath = Path.Combine(AppContext.BaseDirectory, "Data", "Custom", "TutorialCompanions.json");

    private static readonly Lazy<StarterCompanionTable> DefaultTable = new(() => Load(DefaultPath));

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>The table loaded from the file next to the server (loaded on first use).</summary>
    public static StarterCompanionTable Default => DefaultTable.Value;

    public static StarterCompanionTable Load(string path)
    {
        ChoicesFile file = JsonSerializer.Deserialize<ChoicesFile>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException($"{path} is empty.");
        return new StarterCompanionTable(file.Choices.ToDictionary(c => c.ChoiceId, c => c.CompanionId));
    }

    /// <summary>The companion this answer gives, or false for an answer the story doesn't have.</summary>
    public bool TryGetCompanionId(int choiceId, out int companionId) => companionByChoice.TryGetValue(choiceId, out companionId);

    private sealed record ChoicesFile(List<Choice> Choices);

    private sealed record Choice(int ChoiceId, int CompanionId);
}
