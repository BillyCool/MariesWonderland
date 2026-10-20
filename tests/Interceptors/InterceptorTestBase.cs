using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Data;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Interceptors;

/// <summary>
/// Base class for interceptor tests. Provides shared helpers for invoking
/// an interceptor's unary handler directly and constructing empty user stores.
/// </summary>
public abstract class InterceptorTestBase
{
    /// <summary>
    /// Invokes an interceptor's UnaryServerHandler directly, bypassing the gRPC pipeline.
    /// The continuation simply calls <paramref name="continuationBody"/> and returns its result.
    /// </summary>
    protected static Task<TResponse> CallInterceptor<TRequest, TResponse>(
        Interceptor interceptor,
        TRequest request,
        ServerCallContext context,
        Func<TResponse> continuationBody)
        where TRequest : class
        where TResponse : class
    {
        return interceptor.UnaryServerHandler(
            request,
            context,
            (req, ctx) => Task.FromResult(continuationBody()));
    }

    /// <summary>Creates a <see cref="UserDataStore"/> with no users registered.</summary>
    protected static UserDataStore CreateEmptyStore() =>
        new(new DarkMasterMemoryDatabase(), TimeProvider.System);

    /// <summary>
    /// Creates a <see cref="UserDataStore"/> pre-loaded with the given user database.
    /// </summary>
    protected static UserDataStore CreateStoreWithUser(long userId, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(new DarkMasterMemoryDatabase(), TimeProvider.System);
        store.Set(userId, userDb);
        return store;
    }

    /// <summary>Shorthand for <see cref="FakeServerCallContext.For"/>.</summary>
    protected static FakeServerCallContext ContextFor(long userId) =>
        FakeServerCallContext.For(userId);
}
