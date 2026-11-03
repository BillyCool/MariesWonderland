using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Interceptors;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Interceptors;

public class MasterDataVersionInterceptorTests(MasterDatabaseFixture fixture) : InterceptorTestBase
{
    private const string ServerVersion = "20240404193219_100";

    private MasterDataVersionInterceptor CreateInterceptor()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        return new(new MasterDataHolder(new MasterDataSnapshot(fixture.MasterDb, ServerVersion)), NullLogger<MasterDataVersionInterceptor>.Instance);
    }

    private static FakeServerCallContext Call(string? clientVersion, bool loggedIn)
    {
        FakeServerCallContext context = clientVersion is null
            ? FakeServerCallContext.Anonymous()
            : FakeServerCallContext.Anonymous(null, new Metadata.Entry("x-apb-master-data-hash", clientVersion));
        if (loggedIn)
        {
            context.SetUserId(1);
        }
        return context;
    }

    private static async Task<RpcException?> Run(MasterDataVersionInterceptor interceptor, FakeServerCallContext context)
    {
        try
        {
            await CallInterceptor(interceptor, new SetUserNameRequest(), context, () => new SetUserNameResponse());
            return null;
        }
        catch (RpcException error)
        {
            return error;
        }
    }

    [Fact]
    public async Task StaleVersion_SendsTheClientToTheTitle()
    {
        RpcException? error = await Run(CreateInterceptor(), Call("20240404193219_99", loggedIn: true));

        Assert.NotNull(error);
        Assert.Equal("3", error.Trailers.GetValue("x-apb-screen-transition-type"));
        Assert.Equal("104000", error.Trailers.GetValue("x-apb-message-code"));
        Assert.Equal("104000", error.Trailers.GetValue("x-apb-message-text-id"));
    }

    [Fact]
    public async Task CurrentVersion_Passes()
        => Assert.Null(await Run(CreateInterceptor(), Call(ServerVersion, loggedIn: true)));

    [Fact]
    public async Task NoVersionHeader_Passes()
        => Assert.Null(await Run(CreateInterceptor(), Call(null, loggedIn: true)));

    [Fact]
    public async Task EmptyVersionHeader_Passes()
        => Assert.Null(await Run(CreateInterceptor(), Call("", loggedIn: true)));

    [Theory]
    [InlineData("/apb.api.user.UserService/GetAndroidArgs")]
    [InlineData("/apb.api.user.UserService/Auth")]
    [InlineData("/apb.api.data.DataService/GetLatestMasterDataVersion")]
    public async Task StaleVersion_PassesTheTitleFlowCalls(string method)
    {
        FakeServerCallContext context = Call("20240404193219_99", loggedIn: true);
        context.MethodName = method;

        Assert.Null(await Run(CreateInterceptor(), context));
    }

    [Fact]
    public async Task StaleVersionBeforeLogin_Passes()
        => Assert.Null(await Run(CreateInterceptor(), Call("20240404193219_99", loggedIn: false)));
}
