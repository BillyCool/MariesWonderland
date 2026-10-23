using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using System.Text.Json;

namespace MariesWonderland.Interceptors;

/// <summary>
/// gRPC interceptor that persists the user's in-memory database to a timestamped JSON file in
/// the Saves/ directory after every API call. Runs asynchronously on a background thread so it
/// does not block the response. Useful for debugging and post-mortem analysis.
/// </summary>
public class AutoSaveInterceptor(UserDataStore store, ILogger<AutoSaveInterceptor> logger, TimeProvider timeProvider) : Interceptor
{
    private readonly TimeProvider _timeProvider = timeProvider;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string SavesDirectory = Path.Combine(AppContext.BaseDirectory, "Saves");

    /// <summary>
    /// Intercepts gRPC calls to auto-save the user's database to disk after each request.
    /// </summary>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        TResponse response = await continuation(request, context);

        if (context.TryGetUserId(out long userId) && store.TryGet(userId, out DarkUserMemoryDatabase userDb))
        {
            string[] parts = context.Method.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string methodSuffix = parts.Length >= 2 ? $"{parts[^2]}_{parts[^1]}" : context.Method.TrimStart('/').Replace('/', '_');
            // Serialized here, still under the user's lock (UserLockInterceptor); only the file write is left to the background.
            string json = JsonSerializer.Serialize(userDb, JsonOptions);
            long savedUserId = userDb.UserId;
            _ = Task.Run(() => SaveUser(savedUserId, json, methodSuffix));
        }

        return response;
    }

    /// <summary>
    /// Writes a user's serialized database to a timestamped JSON file.
    /// </summary>
    private void SaveUser(long userId, string json, string methodSuffix)
    {
        try
        {
            Directory.CreateDirectory(SavesDirectory);
            string timestamp = _timeProvider.GetUtcNow().UtcDateTime.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(SavesDirectory, $"{userId}_{timestamp}_{methodSuffix}.json");
            File.WriteAllText(filePath, json);
            //logger.LogDebug("Auto-saved user {UserId} to {FilePath}", userDb.UserId, filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AutoSaveInterceptor failed to save user {UserId}", userId);
        }
    }
}
