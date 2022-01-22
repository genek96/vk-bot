namespace VkLongPolling.Configuration;

public record ClientSettings(
    string ServerAddress,
    string GroupId,
    string Token,
    string? ApiVersion = null,
    int waitTimeout = 25
);