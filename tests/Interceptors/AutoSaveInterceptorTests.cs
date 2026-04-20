using MariesWonderland.Data;
using MariesWonderland.Interceptors;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MariesWonderland.Tests.Interceptors;

public class AutoSaveInterceptorTests : InterceptorTestBase
{
    private static readonly ILogger<AutoSaveInterceptor> Logger = NullLogger<AutoSaveInterceptor>.Instance;

    private static AutoSaveInterceptor CreateInterceptor(UserDataStore store) =>
        new(store, Logger);

    /// <summary>
    /// Verifies that the interceptor returns the continuation's response unchanged.
    /// </summary>
    [Fact]
    public async Task ContinuationResultIsReturned()
    {
        var store = CreateEmptyStore();
        var interceptor = CreateInterceptor(store);
        var context = FakeServerCallContext.For(userId: 1);
        var expected = new SetUserNameResponse();

        var result = await CallInterceptor(
            interceptor,
            new SetUserNameRequest(),
            context,
            () => expected);

        Assert.Same(expected, result);
    }

    /// <summary>
    /// Verifies that when the userId is valid but the user is not in the store,
    /// the interceptor still returns the response without crashing.
    /// </summary>
    [Fact]
    public async Task UnknownUser_ContinuationStillCalled()
    {
        var store = CreateEmptyStore();
        var interceptor = CreateInterceptor(store);
        var context = FakeServerCallContext.For(userId: 999);
        var expected = new SetUserNameResponse();

        var result = await CallInterceptor(
            interceptor,
            new SetUserNameRequest(),
            context,
            () => expected);

        Assert.Same(expected, result);
    }
}
