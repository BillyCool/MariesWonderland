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
public class AutoSaveInterceptor(UserDataStore store, ILogger<AutoSaveInterceptor> logger) : Interceptor
{
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

        long userId = context.GetUserId();
        if (userId > 0 && store.TryGet(userId, out DarkUserMemoryDatabase userDb))
        {
            string[] parts = context.Method.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string methodSuffix = parts.Length >= 2 ? $"{parts[^2]}_{parts[^1]}" : context.Method.TrimStart('/').Replace('/', '_');
            _ = Task.Run(() => SaveUser(userDb, methodSuffix));
        }

        return response;
    }

    /// <summary>
    /// Serializes and writes a user's database to a timestamped JSON file.
    /// </summary>
    private void SaveUser(DarkUserMemoryDatabase userDb, string methodSuffix)
    {
        try
        {
            Directory.CreateDirectory(SavesDirectory);
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(SavesDirectory, $"{userDb.UserId}_{timestamp}_{methodSuffix}.json");
            string json = JsonSerializer.Serialize(userDb, JsonOptions);
            File.WriteAllText(filePath, json);
            //logger.LogDebug("Auto-saved user {UserId} to {FilePath}", userDb.UserId, filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AutoSaveInterceptor failed to save user {UserId}", userDb.UserId);
        }
    }
}
