using MariesWonderland.Data;
using MariesWonderland.Interceptors;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;

namespace MariesWonderland.Tests.Data;

/// <summary>
/// Verifies that server time comes from the injected <see cref="TimeProvider"/>, so tests
/// (and a future dev clock) control what "now" is.
/// </summary>
public class TimeProviderTests
{
    private static readonly DateTimeOffset Start = new(2023, 6, 1, 4, 59, 0, TimeSpan.FromHours(9));

    [Fact]
    public void Session_ExpiresWhenClockPassesTtl()
    {
        FakeTimeProvider time = new(Start);
        UserDataStore store = new(time);
        UserSession session = store.CreateSession(userId: 1, TimeSpan.FromHours(24));

        time.Advance(TimeSpan.FromHours(23));
        Assert.True(store.TryResolveSession(session.SessionKey, out _));

        time.Advance(TimeSpan.FromHours(2));
        Assert.False(store.TryResolveSession(session.SessionKey, out _));
    }

    [Fact]
    public void NewUser_IsStampedWithProviderTime()
    {
        FakeTimeProvider time = new(Start);
        UserDataStore store = new(time);

        (long userId, _) = store.RegisterOrGetUser("time-test-uuid", new DarkMasterMemoryDatabase());

        Assert.True(store.TryGet(userId, out DarkUserMemoryDatabase userDb));
        Assert.Equal(Start.ToUnixTimeMilliseconds(), userDb.EntityIUser[0].RegisterDatetime);
    }

    [Fact]
    public async Task ResponseDatetimeTrailer_UsesProviderTime()
    {
        FakeTimeProvider time = new(Start);
        CommonHeaderInterceptor interceptor = new(time);
        FakeServerCallContext context = FakeServerCallContext.For(userId: 1);

        await interceptor.UnaryServerHandler(
            new SetUserNameRequest(),
            context,
            (request, ctx) => Task.FromResult(new SetUserNameResponse()));

        Assert.Equal(
            Start.ToUnixTimeMilliseconds().ToString(),
            context.ResponseTrailers.Single(m => m.Key == "x-apb-response-datetime").Value);
    }
}
