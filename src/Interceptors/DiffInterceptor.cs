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
/// gRPC interceptor that computes what each logged-in call changed in the caller's user data, from snapshots
/// before and after the handler runs. Responses that declare a <c>DiffUserData</c> field get the client diff (so
/// services never fill it themselves), and the change set is written to the user database
/// (<see cref="UserDataStore.SaveChanges(long, Dictionary{string, TableSnapshot}, Dictionary{string, TableSnapshot}, DarkUserMemoryDatabase)"/>).
/// It runs inside <c>UserLockInterceptor</c>, so nothing else changes the user in between.
/// When the handler throws, whatever it changed is undone (<see cref="UndoFailedCall"/>), since a failed call carries
/// no diff and saves nothing.
/// Only calls with a session are tracked: calls made before login (RegisterUser, Auth, TransferUser…) save
/// themselves, and the client loads its full state with GetUserData right after logging in.
/// </summary>
public class DiffInterceptor(UserDataStore store, ILogger<DiffInterceptor> logger) : Interceptor
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> PropertyCache = new();

    /// <summary>
    /// Intercepts every unary gRPC call. If the caller is logged in, snapshots their data before and after the
    /// handler, fills the response's DiffUserData (when it has one) and saves the changes.
    /// </summary>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        PropertyInfo? diffProp = PropertyCache.GetOrAdd(typeof(TResponse), static t => t.GetProperty(nameof(AuthUserResponse.DiffUserData)));

        // No session, or nothing to do with a delta (no diff field and no user database): pass through.
        if (!context.TryGetUserId(out long userId) || (diffProp is null && !store.IsPersistent))
        {
            return await continuation(request, context);
        }

        // A user the handler creates counts as all new.
        bool wasLoaded = store.TryGet(userId, out DarkUserMemoryDatabase? beforeDb);
        Dictionary<string, TableSnapshot> before = wasLoaded ? UserDataDiffBuilder.Snapshot(beforeDb!) : [];

        TResponse response;
        try
        {
            response = await continuation(request, context);
        }
        catch
        {
            // A failed call sends the client no diff and saves nothing, so any change the handler made before it threw
            // must not stay in memory either: the next full load would show the client a change that was never saved.
            UndoFailedCall(userId, wasLoaded, before);
            throw;
        }

        if (!store.TryGet(userId, out DarkUserMemoryDatabase userDb))
        {
            return response;
        }

        Dictionary<string, TableSnapshot> after = UserDataDiffBuilder.Snapshot(userDb);

        try
        {
            if (diffProp?.GetValue(response) is MapField<string, DiffData> mapField)
            {
                Dictionary<string, DiffData> delta = UserDataDiffBuilder.ClientDiff(before, after);
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

        store.SaveChanges(userId, before, after, userDb);
        context.SetUserChanges(new UserChanges(userId, before, after));
        return response;
    }

    /// <summary>
    /// Puts the user back to how they were before a failed call. A user that was in memory gets their changed tables
    /// rebuilt from the snapshot. A user the call loaded or created is dropped from memory instead, so the next call
    /// loads the last saved state.
    /// </summary>
    private void UndoFailedCall(long userId, bool wasLoaded, Dictionary<string, TableSnapshot> before)
    {
        try
        {
            if (!wasLoaded)
            {
                store.Forget(userId);
                return;
            }

            if (store.TryGet(userId, out DarkUserMemoryDatabase userDb))
            {
                int restored = UserDataDiffBuilder.Restore(userDb, before);
                if (restored > 0)
                {
                    logger.LogInformation("Undid {Count} changed table(s) of user {UserId} after a failed call", restored, userId);
                }
            }
        }
        catch (Exception ex)
        {
            // Last resort: drop the user so nothing half-changed is served or saved.
            logger.LogError(ex, "Undoing a failed call for user {UserId} failed; dropping them from memory", userId);
            store.Forget(userId);
        }
    }
}
