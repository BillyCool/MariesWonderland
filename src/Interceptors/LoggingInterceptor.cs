using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.User;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MariesWonderland.Interceptors;

/// <summary>
/// gRPC interceptor that logs every request/response pair. Writes compact JSON summaries to the
/// configured logger at Debug level and full indented payloads to timestamped files in the GRPC/
/// directory. Large fields (DiffUserData, UserDataJson, TableName) are stripped from log output
/// to keep console logs readable.
/// </summary>
public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    private static readonly List<string> ExcludedPropertyNames = [
        nameof(AuthUserResponse.DiffUserData),
        nameof(TableNameList.TableName),
        nameof(UserDataGetResponse.UserDataJson)
        ];

    private static readonly JsonSerializerOptions IndentedOptions = new() { WriteIndented = true };
    private static readonly string GrpcDirectory = Path.Combine(AppContext.BaseDirectory, "GRPC");

    /// <summary>
    /// Intercepts gRPC calls to log request/response JSON and write full payloads to disk.
    /// </summary>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        string methodName = context.Method;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("[GRPC] >> {Method}  (request)", methodName);
            logger.LogDebug("{Json}", SerializeForLog(request));
        }

        TResponse response = await continuation(request, context);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("[GRPC] << {Method}  (response)", methodName);
            logger.LogDebug("{Json}", SerializeForLog(response));
        }

        _ = Task.Run(() => WriteToDisk(methodName, request, response));

        return response;
    }

    /// <summary>
    /// Writes a timestamped JSON file containing the full request and response for a gRPC call.
    /// </summary>
    private void WriteToDisk<TRequest, TResponse>(string methodName, TRequest request, TResponse response)
    {
        try
        {
            string[] parts = methodName.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string methodSuffix = parts.Length >= 2 ? $"{parts[^2]}_{parts[^1]}" : methodName.TrimStart('/').Replace('/', '_');
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            string fileName = $"{timestamp}_{methodSuffix}.json";

            Directory.CreateDirectory(GrpcDirectory);
            string filePath = Path.Combine(GrpcDirectory, fileName);

            StringBuilder sb = new();
            sb.AppendLine("{");
            sb.AppendLine($"  \"method\": {JsonSerializer.Serialize(methodName)},");
            sb.AppendLine($"  \"timestamp\": \"{DateTime.UtcNow:O}\",");
            sb.AppendLine($"  \"request\": {SerializeFull(request)},");
            sb.AppendLine($"  \"response\": {SerializeFull(response)}");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LoggingInterceptor failed to write GRPC log for {Method}", methodName);
        }
    }

    /// <summary>
    /// Serializes an object to indented JSON, using protobuf JSON format for IMessage types.
    /// </summary>
    private static string SerializeFull(object? obj)
    {
        if (obj is null) return "null";
        if (obj is IMessage message)
        {
            string json = JsonFormatter.Default.Format(message);
            // Re-format with indentation for readability
            try
            {
                JsonNode? node = JsonNode.Parse(json);
                return node?.ToJsonString(IndentedOptions) ?? json;
            }
            catch { return json; }
        }
        return JsonSerializer.Serialize(obj, IndentedOptions);
    }

    /// <summary>
    /// Serializes an object to compact JSON for log output, stripping excluded properties.
    /// </summary>
    private static string SerializeForLog(object obj)
    {
        string json = obj is IMessage message
            ? JsonFormatter.Default.Format(message)
            : JsonSerializer.Serialize(obj);

        return RemovePropertiesFromJson(json);
    }

    /// <summary>
    /// Removes excluded property names from a JSON string.
    /// </summary>
    private static string RemovePropertiesFromJson(string json)
    {
        try
        {
            JsonNode? node = JsonNode.Parse(json);
            if (node is null) return json;

            RemoveProperties(node);

            // Use compact JSON (no indentation) to match prior logging style.
            return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            // If parsing fails for any reason, return the original JSON so logging still occurs.
            return json;
        }
    }

    /// <summary>
    /// Recursively removes excluded properties from a JsonNode tree.
    /// </summary>
    private static void RemoveProperties(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            // Iterate over a snapshot of the keys because we'll be mutating the object.
            foreach (var key in obj.Select(kvp => kvp.Key).ToList())
            {
                if (ExcludedPropertyNames.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    obj.Remove(key);
                }
                else
                {
                    RemoveProperties(obj[key]);
                }
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                RemoveProperties(item);
            }
        }
        // Primitives (JsonValue) do not contain nested properties; nothing to do.
    }
}