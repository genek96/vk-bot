namespace VkLongPolling.Models;

public record SessionInfo(string Server, string Key, string Ts);

internal record ErrorDetails(int ErrorCode, string ErrorMsg);

internal record ServerResponse<T>(T? Response, ErrorDetails? Error);

internal record Request(string Url, HttpContent? Content);
