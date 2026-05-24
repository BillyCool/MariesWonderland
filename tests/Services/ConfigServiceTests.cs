using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Proto.Config;
using MariesWonderland.Tests.Infrastructure;
using ConfigServiceImpl = MariesWonderland.Services.ConfigService;

namespace MariesWonderland.Tests.Services;

public class ConfigServiceTests
{
    [Fact]
    public async Task GetReviewServerConfig_ReturnsEmptyResponseWithNoConfiguredSections()
    {
        ConfigServiceImpl service = new();

        GetReviewServerConfigResponse response = await service.GetReviewServerConfig(
            new Empty(),
            FakeServerCallContext.For(userId: 1));

        Assert.NotNull(response);
        Assert.Null(response.Api);
        Assert.Null(response.Octo);
        Assert.Null(response.WebView);
        Assert.Null(response.MasterData);
        Assert.Empty(response.DiffUserData);
    }

    [Fact]
    public async Task GetReviewServerConfig_ReturnsFreshEmptyResponseOnEachCall()
    {
        ConfigServiceImpl service = new();

        GetReviewServerConfigResponse firstResponse = await service.GetReviewServerConfig(
            new Empty(),
            FakeServerCallContext.For(userId: 1));
        firstResponse.Api = new ApiConfig
        {
            Hostname = "review.example",
            Port = 443
        };
        firstResponse.DiffUserData.Add("patched", new Proto.Data.DiffData());

        GetReviewServerConfigResponse secondResponse = await service.GetReviewServerConfig(
            new Empty(),
            FakeServerCallContext.For(userId: 999));

        Assert.NotSame(firstResponse, secondResponse);
        Assert.Null(secondResponse.Api);
        Assert.Null(secondResponse.Octo);
        Assert.Null(secondResponse.WebView);
        Assert.Null(secondResponse.MasterData);
        Assert.Empty(secondResponse.DiffUserData);
    }
}
