using MariesWonderland.Interceptors;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Interceptors;

public class CommonHeaderInterceptorTests : InterceptorTestBase
{
    private static readonly CommonHeaderInterceptor Interceptor = new();

    /// <summary>
    /// Verifies that after handling a unary call the interceptor adds the
    /// x-apb-response-datetime trailer to the response metadata.
    /// </summary>
    [Fact]
    public async Task AddsResponseDatetimeTrailer()
    {
        var context = FakeServerCallContext.For(userId: 1);

        await CallInterceptor(Interceptor,
            new SetUserNameRequest(),
            context,
            () => new SetUserNameResponse());

        var trailer = context.ResponseTrailers
            .FirstOrDefault(m => m.Key == "x-apb-response-datetime");
        Assert.NotNull(trailer);
    }

    /// <summary>
    /// Verifies that the x-apb-response-datetime trailer value is a valid
    /// Unix timestamp in milliseconds and is within 5 seconds of the current time.
    /// </summary>
    [Fact]
    public async Task ResponseDatetimeIsValidRecentUnixTimestamp()
    {
        var context = FakeServerCallContext.For(userId: 1);
        var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await CallInterceptor(Interceptor,
            new SetUserNameRequest(),
            context,
            () => new SetUserNameResponse());

        var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var trailer = context.ResponseTrailers
            .First(m => m.Key == "x-apb-response-datetime");
        var timestamp = long.Parse(trailer.Value);

        Assert.InRange(timestamp, before, after + 5000);
    }

    /// <summary>
    /// Verifies that the interceptor returns the continuation's response unchanged.
    /// </summary>
    [Fact]
    public async Task ResponseIsReturnedUnchanged()
    {
        var context = FakeServerCallContext.For(userId: 1);
        var expected = new SetUserNameResponse();

        var result = await CallInterceptor(Interceptor,
            new SetUserNameRequest(),
            context,
            () => expected);

        Assert.Same(expected, result);
    }
}
