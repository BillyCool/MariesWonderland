using MariesWonderland.Data;
using MariesWonderland.MasterEditing;
using MariesWonderland.Models.Entities;

namespace MariesWonderland.Web.Services;

/// <summary>
/// A kind of dated master data row the easy-mode schedule screen edits: which table, which columns hold its start
/// and end (unix ms), and how to name a row.
/// </summary>
public sealed record ScheduleKind(
    string Slug,
    string Title,
    string Description,
    MasterTable Table,
    string StartColumn,
    string EndColumn,
    Func<object, DarkMasterMemoryDatabase, GameText, string> Name,
    Func<object, string>? Group = null)
{
    public MasterColumn Start => Table.Columns.Single(c => c.Name == StartColumn);
    public MasterColumn End => Table.Columns.Single(c => c.Name == EndColumn);
}

public static class ScheduleKinds
{
    public static IReadOnlyList<ScheduleKind> All { get; } =
    [
        new("events", "Events", "Event quest chapters: story events, subquests, EX character, Subjugation, Exploration, dailies, guerrillas.",
            MasterTables.Get<EntityMEventQuestChapter>(), nameof(EntityMEventQuestChapter.StartDatetime), nameof(EntityMEventQuestChapter.EndDatetime),
            (row, db, text) => text.EventChapter((EntityMEventQuestChapter)row),
            row => ((EntityMEventQuestChapter)row).EventQuestType.ToString()),
        new("shops", "Shops", "Shop tabs and exchanges.",
            MasterTables.Get<EntityMShop>(), nameof(EntityMShop.StartDatetime), nameof(EntityMShop.EndDatetime),
            (row, db, text) => text.Shop((EntityMShop)row),
            row => ((EntityMShop)row).ShopGroupType.ToString()),
        new("home-banners", "Home banners", "The banners on the home screen (they link to summons, events and shops).",
            MasterTables.Get<EntityMMomBanner>(), nameof(EntityMMomBanner.StartDatetime), nameof(EntityMMomBanner.EndDatetime),
            (row, db, text) => BannerName((EntityMMomBanner)row),
            row => ((EntityMMomBanner)row).DestinationDomainType.ToString()),
        new("subjugation", "Subjugation (Big Hunt)", "Big Hunt challenge weeks.",
            MasterTables.Get<EntityMBigHuntSchedule>(), nameof(EntityMBigHuntSchedule.ChallengeStartDatetime), nameof(EntityMBigHuntSchedule.ChallengeEndDatetime),
            (row, db, text) => $"Schedule {((EntityMBigHuntSchedule)row).BigHuntScheduleId}"),
        new("arena", "Arena seasons", "PvP seasons. Use the Arena page to add a run of new seasons.",
            MasterTables.Get<EntityMPvpSeason>(), nameof(EntityMPvpSeason.SeasonStartDatetime), nameof(EntityMPvpSeason.SeasonEndDatetime),
            (row, db, text) =>
            {
                EntityMPvpSeason season = (EntityMPvpSeason)row;
                return $"Season {season.PvpSeasonId}{(season.IsInvalid ? " (invalid)" : "")}";
            }),
        new("fate-board", "Fate Board seasons", "Labyrinth seasons of each Fate Board chapter.",
            MasterTables.Get<EntityMEventQuestLabyrinthSeason>(), nameof(EntityMEventQuestLabyrinthSeason.StartDatetime), nameof(EntityMEventQuestLabyrinthSeason.EndDatetime),
            (row, db, text) =>
            {
                EntityMEventQuestLabyrinthSeason season = (EntityMEventQuestLabyrinthSeason)row;
                EntityMEventQuestChapter? chapter = db.EntityMEventQuestChapter.Find(season.EventQuestChapterId);
                return $"{(chapter == null ? $"Chapter {season.EventQuestChapterId}" : text.EventChapter(chapter))}, season {season.SeasonNumber}";
            },
            row => ((EntityMEventQuestLabyrinthSeason)row).EventQuestChapterId.ToString()),
        new("login-bonus", "Login bonuses", "Login bonus calendars.",
            MasterTables.Get<EntityMLoginBonus>(), nameof(EntityMLoginBonus.StartDatetime), nameof(EntityMLoginBonus.EndDatetime),
            (row, db, text) => ((EntityMLoginBonus)row).LoginBonusAssetName),
        new("maintenance", "Maintenance blocks", "Function blocks and downtimes (group says what is blocked).",
            MasterTables.Get<EntityMMaintenance>(), nameof(EntityMMaintenance.StartDatetime), nameof(EntityMMaintenance.EndDatetime),
            (row, db, text) => $"Maintenance {((EntityMMaintenance)row).MaintenanceId}",
            row => $"Group {((EntityMMaintenance)row).MaintenanceGroupId}"),
    ];

    public static ScheduleKind? Find(string? slug) => All.FirstOrDefault(k => k.Slug == slug);

    private static string BannerName(EntityMMomBanner banner)
        => string.IsNullOrEmpty(banner.BannerAssetName) ? $"Banner {banner.MomBannerId} → {banner.DestinationDomainType} {banner.DestinationDomainId}" : $"{banner.BannerAssetName} → {banner.DestinationDomainType} {banner.DestinationDomainId}";
}
