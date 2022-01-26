using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class UpdateEventHandler
{
    public UpdateEventHandler(
        Predicate<IUpdateEventObject> canHandleEvent,
        Func<IUpdateEventObject, Func<int, string, Keyboard?, Task<bool>>, Task> handleAsync
    )
    {
        CanHandleEvent = canHandleEvent;
        HandleAsync = handleAsync;
    }

    public Predicate<IUpdateEventObject> CanHandleEvent { get; init; }
    public Func<IUpdateEventObject, Func<int, string, Keyboard?, Task<bool>>, Task> HandleAsync { get; init; }
}