using VkLongPolling.Client;
using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class UpdateEventHandler
{
    public UpdateEventHandler(
        Predicate<IUpdateEventObject> canHandleEvent,
        Func<IUpdateEventObject, IResponder, Task> handleAsync
    )
    {
        CanHandleEvent = canHandleEvent;
        HandleAsync = handleAsync;
    }

    public Predicate<IUpdateEventObject> CanHandleEvent { get; init; }
    public Func<IUpdateEventObject, IResponder, Task> HandleAsync { get; init; }
}