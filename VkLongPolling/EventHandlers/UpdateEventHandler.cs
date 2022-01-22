using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class UpdateEventHandler
{
    public UpdateEventHandler(
        Predicate<IUpdateEventObject> canHandleEvent,
        Func<IUpdateEventObject, Task> handleAsync
    )
    {
        CanHandleEvent = canHandleEvent;
        HandleAsync = handleAsync;
    }

    public Predicate<IUpdateEventObject> CanHandleEvent { get; init; }
    public Func<IUpdateEventObject, Task> HandleAsync { get; init; }
}