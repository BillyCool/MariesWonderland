using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.IndividualPop;
using MariesWonderland.Tests.Infrastructure;
using IndividualPopServiceImpl = MariesWonderland.Services.IndividualPopService;

namespace MariesWonderland.Tests.Services;

public class IndividualPopServiceTests
{
    [Fact]
    public async Task GetUnreadPop_ReturnsEmptyResponseWithoutUnreadPopups()
    {
        IndividualPopServiceImpl service = CreateService();

        GetUnreadPopResponse response = await service.GetUnreadPop(
            new Empty(),
            FakeServerCallContext.For(userId: 1));

        Assert.NotNull(response);
        Assert.Empty(response.UnreadPop);
        Assert.Empty(response.DiffUserData);
    }

    [Fact]
    public async Task GetUnreadPop_EachCallReturnsFreshEmptyCollections()
    {
        IndividualPopServiceImpl service = CreateService();

        GetUnreadPopResponse firstResponse = await service.GetUnreadPop(
            new Empty(),
            FakeServerCallContext.For(userId: 1));
        firstResponse.UnreadPop.Add("patched");
        firstResponse.DiffUserData.Add("patched", new DiffData());

        GetUnreadPopResponse secondResponse = await service.GetUnreadPop(
            new Empty(),
            FakeServerCallContext.For(userId: 999));

        Assert.NotSame(firstResponse, secondResponse);
        Assert.Empty(secondResponse.UnreadPop);
        Assert.Empty(secondResponse.DiffUserData);
    }

    private static IndividualPopServiceImpl CreateService() => new();
}
