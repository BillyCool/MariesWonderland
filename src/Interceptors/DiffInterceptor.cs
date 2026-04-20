using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.User;
using System.Collections.Concurrent;
using System.Reflection;

namespace MariesWonderland.Interceptors;

/// <summary>
/// gRPC interceptor that automatically computes and attaches <c>DiffUserData</c> to every response
/// that declares the field. Takes a before-snapshot of the user's database prior to service execution,
/// then computes the delta after the service mutates state. This means individual services never need
/// to populate DiffUserData manually.
/// Special-cases the RegisterUser flow where no userId is available in request headers: extracts the
/// newly assigned userId from the response to perform a full-state diff against an empty baseline.
/// </summary>
public class DiffInterceptor(UserDataStore store, ILogger<DiffInterceptor> logger) : Interceptor
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> PropertyCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> UserIdPropertyCache = new();

    /// <summary>
    /// Intercepts every unary gRPC call. If the response type has a DiffUserData map field,
    /// snapshots user state before execution, runs the handler, then populates the diff.
    /// </summary>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        long userId = context.GetUserId();
        PropertyInfo? diffProp = PropertyCache.GetOrAdd(typeof(TResponse), static t => t.GetProperty(nameof(AuthUserResponse.DiffUserData)));

        // Response type has no DiffUserData property — pass through without snapshotting
        if (diffProp is null)
        {
            return await continuation(request, context);
        }

        if (userId != 0)
        {
            // Normal path: userId is known from request headers — snapshot before, diff after
            Dictionary<string, string> before = store.TryGet(userId, out DarkUserMemoryDatabase userDb)
                ? UserDataDiffBuilder.Snapshot(userDb)
                : [];

            TResponse response = await continuation(request, context);

            try
            {
                if (diffProp.GetValue(response) is MapField<string, DiffData> mapField)
                {
                    Dictionary<string, DiffData> delta = UserDataDiffBuilder.Delta(before, userDb);
                    foreach ((string key, DiffData value) in delta)
                    {
                        mapField[key] = value;
                    }

                    if (delta.Count > 0)
                    {
                        string[] names = [.. delta.Keys];
                        Array.Sort(names, StringComparer.Ordinal);
                        context.ResponseTrailers.Add("x-apb-update-user-data-names", string.Join(",", names));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DiffInterceptor failed to populate DiffUserData on {Method}", context.Method);
            }

            return response;
        }
        else
        {
            // RegisterUser path: userId=0 in headers because the user doesn't exist yet.
            // Run the handler first, then extract the newly assigned userId from the response
            // and diff against an empty baseline to send all initial state to the client.
            TResponse response = await continuation(request, context);

            try
            {
                PropertyInfo? userIdProp = UserIdPropertyCache.GetOrAdd(typeof(TResponse), static t => t.GetProperty("UserId"));

                if (userIdProp?.GetValue(response) is long newUserId && newUserId != 0)
                {
                    if (store.TryGet(newUserId, out DarkUserMemoryDatabase userDb))
                    {
                        // Only populate if the map is empty (service didn't set it manually)
                        if (diffProp.GetValue(response) is MapField<string, DiffData> mapField && mapField.Count == 0)
                        {
                            foreach ((string key, DiffData value) in UserDataDiffBuilder.Delta([], userDb))
                            {
                                mapField[key] = value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DiffInterceptor failed to populate DiffUserData on {Method}", context.Method);
            }

            return response;
        }
    }
}
