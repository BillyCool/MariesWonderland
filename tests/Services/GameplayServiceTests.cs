using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.GamePlay;
using MariesWonderland.Proto.Gacha;
using MariesWonderland.Tests.Infrastructure;
using GameplayServiceImpl = MariesWonderland.Services.GameplayService;

namespace MariesWonderland.Tests.Services;

public class GameplayServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task CheckBeforeGamePlay_ReturnsDefaultResponseWithoutUnreadPopups()
    {
        GameplayServiceImpl service = CreateService();

        CheckBeforeGamePlayResponse response = await service.CheckBeforeGamePlay(
            new CheckBeforeGamePlayRequest
            {
                Tr = "trace-id",
                VoiceClientSystemLanguageTypeId = 1,
                TextClientSystemLanguageTypeId = 2
            },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.False(response.IsExistUnreadPop);
        Assert.Empty(response.MenuGachaBadgeInfo);
        Assert.Empty(response.DiffUserData);
    }

    [Fact]
    public async Task CheckBeforeGamePlay_EachCallReturnsFreshEmptyCollections()
    {
        GameplayServiceImpl service = CreateService();

        CheckBeforeGamePlayResponse firstResponse = await service.CheckBeforeGamePlay(
            new CheckBeforeGamePlayRequest(),
            FakeServerCallContext.For(UserId));
        firstResponse.DiffUserData.Add("changed", new DiffData());
        firstResponse.MenuGachaBadgeInfo.Add(new MenuGachaBadgeInfo());

        CheckBeforeGamePlayResponse secondResponse = await service.CheckBeforeGamePlay(
            new CheckBeforeGamePlayRequest(),
            FakeServerCallContext.For(UserId));

        Assert.NotSame(firstResponse, secondResponse);
        Assert.Empty(secondResponse.MenuGachaBadgeInfo);
        Assert.Empty(secondResponse.DiffUserData);
        Assert.False(secondResponse.IsExistUnreadPop);
    }

    private static GameplayServiceImpl CreateService() => new();
}
