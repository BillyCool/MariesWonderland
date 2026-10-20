using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Interceptors;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Interceptors;

public class SessionInterceptorTests : InterceptorTestBase
{
    private const long UserId = 42L;

    /// <summary>
    /// Runs the interceptor and returns the identity the handler saw, or null if none was resolved.
    /// </summary>
    private static async Task<long?> ResolveIdentity(UserDataStore store, ServerCallContext context)
    {
        long? seen = null;
        await CallInterceptor(new SessionInterceptor(store), new SetUserNameRequest(), context, () =>
        {
            seen = context.TryGetUserId(out long userId) ? userId : null;
            return new SetUserNameResponse();
        });
        return seen;
    }

    [Fact]
    public async Task ValidSession_ResolvesSessionUser()
    {
        UserDataStore store = CreateEmptyStore();
        UserSession session = store.CreateSession(UserId, TimeSpan.FromMinutes(5));

        long? resolved = await ResolveIdentity(store, FakeServerCallContext.Anonymous(session.SessionKey));

        Assert.Equal(UserId, resolved);
    }

    [Fact]
    public async Task NoSessionKey_LeavesCallUnauthenticated()
    {
        long? resolved = await ResolveIdentity(CreateEmptyStore(), FakeServerCallContext.Anonymous());

        Assert.Null(resolved);
    }

    [Fact]
    public async Task UnknownSessionKey_LeavesCallUnauthenticated()
    {
        long? resolved = await ResolveIdentity(CreateEmptyStore(), FakeServerCallContext.Anonymous("session_unknown"));

        Assert.Null(resolved);
    }

    [Fact]
    public async Task ExpiredSession_LeavesCallUnauthenticated()
    {
        UserDataStore store = CreateEmptyStore();
        UserSession session = store.CreateSession(UserId, TimeSpan.FromSeconds(-1));

        long? resolved = await ResolveIdentity(store, FakeServerCallContext.Anonymous(session.SessionKey));

        Assert.Null(resolved);
    }

    [Fact]
    public async Task ClientUserIdHeader_IsIgnored()
    {
        UserDataStore store = CreateEmptyStore();
        UserSession session = store.CreateSession(UserId, TimeSpan.FromMinutes(5));
        FakeServerCallContext context = FakeServerCallContext.Anonymous(
            session.SessionKey,
            new Metadata.Entry("x-apb-user-id", "999"));

        long? resolved = await ResolveIdentity(store, context);

        Assert.Equal(UserId, resolved);
    }

    [Fact]
    public void GetUserId_WithoutResolvedSession_ThrowsUnauthenticated()
    {
        FakeServerCallContext context = FakeServerCallContext.Anonymous(
            headers: new Metadata.Entry("x-apb-user-id", "999"));

        RpcException exception = Assert.Throws<RpcException>(() => context.GetUserId());

        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
    }
}
