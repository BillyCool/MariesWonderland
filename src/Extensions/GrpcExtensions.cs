using MariesWonderland.Services;

namespace MariesWonderland.Extensions;

/// <summary>
/// Registers all gRPC service implementations with the ASP.NET Core endpoint routing pipeline.
/// Each <c>MapGrpcService</c> call wires a service class to its protobuf-defined RPC methods.
/// </summary>
public static class GrpcExtensions
{
    /// <summary>
    /// Registers all gRPC service endpoints on the application.
    /// </summary>
    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<BannerService>();
        app.MapGrpcService<BattleService>();
        app.MapGrpcService<BigHuntService>();
        app.MapGrpcService<CageOrnamentService>();
        app.MapGrpcService<CharacterBoardService>();
        app.MapGrpcService<CharacterService>();
        app.MapGrpcService<CharacterViewerService>();
        app.MapGrpcService<CompanionService>();
        app.MapGrpcService<ConfigService>();
        app.MapGrpcService<ConsumableItemService>();
        app.MapGrpcService<ContentsStoryService>();
        app.MapGrpcService<CostumeService>();
        app.MapGrpcService<DataService>();
        app.MapGrpcService<DeckService>();
        app.MapGrpcService<DokanService>();
        app.MapGrpcService<ExploreService>();
        app.MapGrpcService<FriendService>();
        app.MapGrpcService<GachaService>();
        app.MapGrpcService<GameplayService>();
        app.MapGrpcService<GiftService>();
        app.MapGrpcService<GimmickService>();
        app.MapGrpcService<IndividualPopService>();
        app.MapGrpcService<LabyrinthService>();
        app.MapGrpcService<LoginBonusService>();
        app.MapGrpcService<MaterialService>();
        app.MapGrpcService<MissionService>();
        app.MapGrpcService<MovieService>();
        app.MapGrpcService<NaviCutInService>();
        app.MapGrpcService<NotificationService>();
        app.MapGrpcService<OmikujiService>();
        app.MapGrpcService<PartsService>();
        app.MapGrpcService<PortalCageService>();
        app.MapGrpcService<PvpService>();
        app.MapGrpcService<QuestService>();
        app.MapGrpcService<RewardService>();
        app.MapGrpcService<ShopService>();
        app.MapGrpcService<SideStoryQuestService>();
        app.MapGrpcService<TutorialService>();
        app.MapGrpcService<UserService>();
        app.MapGrpcService<WeaponService>();
        return app;
    }
}
