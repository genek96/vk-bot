using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class UpdateEventHandler
{
    public UpdateEventHandler(
        Predicate<IUpdateEventObject> canHandleEvent,
        Func<IUpdateEventObject, SendResponseFunc, Task> handleAsync
    )
    {
        CanHandleEvent = canHandleEvent;
        HandleAsync = handleAsync;
    }

    public delegate Task<bool> SendResponseFunc(int userId, string message, Keyboard? keyboard);
    public Predicate<IUpdateEventObject> CanHandleEvent { get; init; }
    public Func<IUpdateEventObject, SendResponseFunc, Task> HandleAsync { get; init; }
}