using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MariesWonderland.Interceptors;

/// <summary>
/// Dev tool: appends what each call changed in the caller's data to a daily journal (Journal/yyyy-MM-dd.log), for
/// troubleshooting next to the gRPC log (entries carry the same request id). Every table is covered, server-only ones
/// included: new and removed rows are written whole, changed rows as their changed fields, old → new.
/// The changes come from <see cref="DiffInterceptor"/> (or a call that saves itself, like Auth), so this runs outside
/// it, inside <c>UserLockInterceptor</c>.
/// </summary>
/// <example>
/// <code>
/// 2026-10-24 12:03:45.123 #1f3a9c02 CostumeService/Enhance user 1234567890123456789
///   IUserMaterial
///     + {"materialId":3,"count":1,"firstAcquisitionDatetime":1761300225123,"latestVersion":0}
///     ~ materialId=1: count 5 → 6
///     - {"materialId":2,"count":7,"firstAcquisitionDatetime":1761300100000,"latestVersion":0}
/// </code>
/// </example>
public class RequestJournalInterceptor(ILogger<RequestJournalInterceptor> logger, TimeProvider timeProvider, string? directory = null) : Interceptor
{
    private static readonly Lock WriteLock = new();
    private static readonly JsonSerializerOptions PrintOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static readonly IReadOnlyDictionary<string, string[]> KeyNames = UserTables.All.ToDictionary(t => t.Name, KeyNamesOf);

    /// <summary>Where the journal files go.</summary>
    public string Directory { get; } = directory ?? Path.Combine(AppContext.BaseDirectory, "Journal");

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        TResponse response = await continuation(request, context);

        UserChanges? changes = context.GetUserChanges();
        if (changes == null)
        {
            return response;
        }

        try
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            string entry = Format(now, context.GetRequestId(), ShortMethod(context.Method), changes);
            lock (WriteLock)
            {
                System.IO.Directory.CreateDirectory(Directory);
                File.AppendAllText(Path.Combine(Directory, $"{now:yyyy-MM-dd}.log"), entry);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Writing the request journal for {Method} failed", context.Method);
        }

        return response;
    }

    /// <summary>One journal entry: a header line, then each changed table and its rows.</summary>
    public static string Format(DateTimeOffset time, string requestId, string method, UserChanges changes)
    {
        StringBuilder sb = new();
        sb.Append($"{time:yyyy-MM-dd HH:mm:ss.fff} #{requestId} {method} user {changes.UserId}\n");

        bool any = false;
        foreach (UserTable table in UserTables.All)
        {
            TableSnapshot after = changes.After.GetValueOrDefault(table.Name) ?? TableSnapshot.Empty;
            TableSnapshot before = changes.Before.GetValueOrDefault(table.Name) ?? TableSnapshot.Empty;

            bool tableHeader = false;
            foreach ((string? oldJson, string? newJson) in after.CompareTo(before))
            {
                if (!tableHeader)
                {
                    sb.Append($"  {table.Name}\n");
                    tableHeader = true;
                }

                JsonObject? oldRow = oldJson == null ? null : JsonNode.Parse(oldJson)!.AsObject();
                JsonObject? newRow = newJson == null ? null : JsonNode.Parse(newJson)!.AsObject();
                if (oldRow == null)
                {
                    sb.Append($"    + {Print(WithoutUserId(newRow!))}\n");
                }
                else if (newRow == null)
                {
                    sb.Append($"    - {Print(WithoutUserId(oldRow))}\n");
                }
                else
                {
                    sb.Append($"    ~ {ChangedFields(KeyNames[table.Name], oldRow, newRow)}\n");
                }
            }
            any |= tableHeader;
        }

        if (!any)
        {
            sb.Append("  (no changes)\n");
        }
        return sb.ToString();
    }

    /// <summary>"materialId=1: count 5 → 6, …"; the key part is left out for one-row-per-user tables.</summary>
    private static string ChangedFields(string[] keyNames, JsonObject oldRow, JsonObject newRow)
    {
        List<string> fields = [];
        foreach (string name in newRow.Select(p => p.Key).Union(oldRow.Select(p => p.Key)))
        {
            JsonNode? oldValue = oldRow[name];
            JsonNode? newValue = newRow[name];
            if (name != "userId" && !JsonNode.DeepEquals(oldValue, newValue))
            {
                fields.Add($"{name} {Print(oldValue)} → {Print(newValue)}");
            }
        }

        string changed = string.Join(", ", fields);
        if (keyNames.Length == 0)
        {
            return changed;
        }

        string key = string.Join(", ", keyNames.Select(k => $"{k}={Print(newRow[k])}"));
        return $"{key}: {changed}";
    }

    private static JsonObject WithoutUserId(JsonObject row)
    {
        row.Remove("userId");
        return row;
    }

    private static string Print(JsonNode? node) => node?.ToJsonString(PrintOptions) ?? "null";

    /// <summary>"/apb.api.costume.CostumeService/Enhance" → "CostumeService/Enhance".</summary>
    public static string ShortMethod(string method)
    {
        string[] parts = method.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[^2][(parts[^2].LastIndexOf('.') + 1)..]}/{parts[^1]}" : method;
    }

    /// <summary>A table's key columns as they're named in the row JSON, without the user id.</summary>
    private static string[] KeyNamesOf(UserTable table)
    {
        IReadOnlyList<PropertyInfo> keys = typeof(EntityKey<>).MakeGenericType(table.RowType)
            .GetProperty(nameof(EntityKey<object>.Properties))!.GetValue(null) as IReadOnlyList<PropertyInfo> ?? [];
        return [.. keys.Select(k => JsonNamingPolicy.CamelCase.ConvertName(k.Name)).Where(k => k != "userId")];
    }
}
