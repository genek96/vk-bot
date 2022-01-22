namespace VkLongPolling.Models;

public record LongPollResponse(
    string? Ts,
    UpdateEvent[]? Updates,
    int? Failed
);

public record UpdateEvent(
    string Type,
    IUpdateEventObject Object,
    string GroupId
);

public interface IUpdateEventObject { }

public record UnknownEvent(string Content) : IUpdateEventObject;
public record JoinGroupEvent(int UserId, string JoinType) : IUpdateEventObject;
public record NewMessageEvent(Message Message, ClientInfo ClientInfo) : IUpdateEventObject;

public record Message(
    int Id,
    int Date,
    int PeerId,
    int FromId,
    string Text,
    int? RandomId,
    string? Ref,
    string? RefSource,
    string? Payload
);

public record ClientInfo(
    string[] ButtonActions,
    bool Keyboard,
    bool InlineKeyboard,
    bool Carousel,
    int langId
);