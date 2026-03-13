namespace MariesWonderland.Data;

public record UserSession(string SessionKey, long UserId, DateTime ExpiresAt);
