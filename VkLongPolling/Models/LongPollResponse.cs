namespace VkLongPolling.Models;

public record LongPollResponse(
    string? Ts,
    UpdateEvent[]? Updates,
    int? Failed
);

public record UpdateEvent(
    string Type,
    string Object,
    string GroupId
);