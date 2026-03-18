using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.User;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MariesWonderland.Interceptors;

public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    private static readonly List<string> ExcludedPropertyNames = [
        nameof(AuthUserResponse.DiffUserData),
        nameof(TableNameList.TableName),
        nameof(UserDataGetResponse.UserDataJson)
        ];

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        string methodName = context.Method;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("[GRPC] >> {Method}  (request)", methodName);
            logger.LogDebug("{Json}", Serialize(request));
        }

        TResponse response = await continuation(request, context);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("[GRPC] << {Method}  (response)", methodName);
            logger.LogDebug("{Json}", Serialize(response));
        }

        return response;
    }

    private static string Serialize(object obj)
    {
        string json = obj is IMessage message
            ? JsonFormatter.Default.Format(message)
            : JsonSerializer.Serialize(obj);

        return RemovePropertiesFromJson(json);
    }

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