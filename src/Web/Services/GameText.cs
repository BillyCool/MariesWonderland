using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using Microsoft.Extensions.Options;
using System.Collections.Frozen;
using System.Text;

namespace MariesWonderland.Web.Services;

/// <summary>
/// The game's English text (<c>all_localizations.txt</c>, <c>key:value</c> lines, UTF-16), for showing names in the
/// admin tool. Set <c>Server:Paths:Localization</c> to the file; without it, names fall back to ids.
/// </summary>
public sealed class GameText(IOptions<ServerOptions> options, ILogger<GameText> logger)
{
    private readonly Lazy<FrozenDictionary<string, string>> _text = new(() => Load(options.Value.Paths.Localization, logger));

    public bool IsAvailable => _text.Value.Count > 0;

    public string? Get(string key) => _text.Value.GetValueOrDefault(key);

    private static FrozenDictionary<string, string> Load(string path, ILogger logger)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            if (!string.IsNullOrEmpty(path))
            {
                logger.LogWarning("Localization file {Path} not found; the admin tool shows ids instead of names", path);
            }
            return FrozenDictionary<string, string>.Empty;
        }

        Dictionary<string, string> text = [];
        // The BOM picks UTF-16; files without one are read as UTF-8.
        foreach (string line in File.ReadLines(path, Encoding.UTF8))
        {
            int colon = line.IndexOf(':');
            if (colon > 0)
            {
                // Line breaks in the text are written as a literal backslash-n.
                text.TryAdd(line[..colon], line[(colon + 1)..].Replace(@"\n", " ").Trim());
            }
        }
        return text.ToFrozenDictionary();
    }

    public string Costume(DarkMasterMemoryDatabase masterDb, int costumeId)
    {
        EntityMCostume? costume = masterDb.EntityMCostume.Find(costumeId);
        if (costume == null)
        {
            return $"Costume {costumeId}";
        }

        string prefix = costume.CostumeAssetCategoryType == CostumeAssetCategoryType.ENEMY ? "mt" : "ch";
        string? name = Get($"costume.name.{prefix}{costume.ActorSkeletonId:D3}{costume.AssetVariationId:D3}");
        string? character = Get($"character.name.{costume.CharacterId}");
        return (character, name) switch
        {
            (null, null) => $"Costume {costumeId}",
            (null, _) => name!,
            (_, null) => $"{character} ({costumeId})",
            _ => $"{character}: {name}",
        };
    }

    public string Weapon(DarkMasterMemoryDatabase masterDb, int weaponId)
    {
        EntityMWeapon? weapon = masterDb.EntityMWeapon.Find(weaponId);
        string? name = weapon == null ? null : Get($"weapon.name.wp{(int)weapon.WeaponType:D3}{weapon.AssetVariationId:D3}.1");
        return name is null or "-" ? $"Weapon {weaponId}" : name;
    }

    public string Companion(DarkMasterMemoryDatabase masterDb, int companionId)
    {
        EntityMCompanion? companion = masterDb.EntityMCompanion.Find(companionId);
        string? name = companion == null ? null : Get($"companion.name.cm{companion.ActorSkeletonId:D3}{companion.AssetVariationId:D3}");
        return name ?? $"Companion {companionId}";
    }

    public string Material(int materialId) => Get($"material.name.{materialId}") ?? $"Material {materialId}";

    public string Consumable(DarkMasterMemoryDatabase masterDb, int consumableItemId)
    {
        EntityMConsumableItem? item = masterDb.EntityMConsumableItem.Find(consumableItemId);
        string? name = item == null ? null : Get($"consumable_item.name.{item.AssetCategoryId * 1000 + item.AssetVariationId}");
        return name ?? $"Item {consumableItemId}";
    }

    public string ImportantItem(DarkMasterMemoryDatabase masterDb, int importantItemId)
    {
        EntityMImportantItem? item = masterDb.EntityMImportantItem.Find(importantItemId);
        return Get($"important_item.name.{item?.NameImportantItemTextId ?? importantItemId}") ?? $"Important item {importantItemId}";
    }

    public string EventChapter(EntityMEventQuestChapter chapter)
        => Get($"quest.event.chapter_title.{chapter.NameEventQuestTextId}") ?? $"Event chapter {chapter.EventQuestChapterId}";

    public string Shop(EntityMShop shop) => Get($"shop.name.{shop.NameShopTextId}") ?? $"Shop {shop.ShopId}";

    /// <summary>The name of anything a gift or reward can be.</summary>
    public string Possession(DarkMasterMemoryDatabase masterDb, PossessionType type, int id) => type switch
    {
        PossessionType.COSTUME => Costume(masterDb, id),
        PossessionType.WEAPON => Weapon(masterDb, id),
        PossessionType.COMPANION => Companion(masterDb, id),
        PossessionType.MATERIAL => Material(id),
        PossessionType.CONSUMABLE_ITEM => Consumable(masterDb, id),
        PossessionType.IMPORTANT_ITEM => ImportantItem(masterDb, id),
        PossessionType.PAID_GEM => "Paid gems",
        PossessionType.FREE_GEM => "Gems",
        _ => $"{type} {id}",
    };

    /// <summary>Everything of one possession type an administrator can grant, by name.</summary>
    public IReadOnlyList<(int Id, string Name)> Catalog(DarkMasterMemoryDatabase masterDb, PossessionType type) => type switch
    {
        PossessionType.COSTUME => [.. masterDb.EntityMCostume.Where(c => c.CostumeAssetCategoryType == CostumeAssetCategoryType.NORMAL)
            .Select(c => (c.CostumeId, Costume(masterDb, c.CostumeId))).OrderBy(c => c.Item2)],
        PossessionType.WEAPON => [.. masterDb.EntityMWeapon.Where(w => w.WeaponCategoryType == 1)
            .Select(w => (w.WeaponId, Weapon(masterDb, w.WeaponId))).OrderBy(w => w.Item2)],
        PossessionType.COMPANION => [.. masterDb.EntityMCompanion.Select(c => (c.CompanionId, Companion(masterDb, c.CompanionId))).OrderBy(c => c.Item2)],
        PossessionType.MATERIAL => [.. masterDb.EntityMMaterial.Select(m => (m.MaterialId, Material(m.MaterialId))).OrderBy(m => m.Item2)],
        PossessionType.CONSUMABLE_ITEM => [.. masterDb.EntityMConsumableItem.Select(c => (c.ConsumableItemId, Consumable(masterDb, c.ConsumableItemId))).OrderBy(c => c.Item2)],
        PossessionType.IMPORTANT_ITEM => [.. masterDb.EntityMImportantItem.Select(i => (i.ImportantItemId, ImportantItem(masterDb, i.ImportantItemId))).OrderBy(i => i.Item2)],
        PossessionType.PAID_GEM or PossessionType.FREE_GEM => [(0, Possession(masterDb, type, 0))],
        _ => [],
    };
}
