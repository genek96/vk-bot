using VkLongPolling.Client;
using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class UpdateEventHandler
{
    public UpdateEventHandler(
        Func<IUpdateEventObject, ValueTask<bool>> canHandleEvent,
        Func<IUpdateEventObject, IResponder, Task> handleAsync
    )
    {
        CanHandleEvent = canHandleEvent;
        HandleAsync = handleAsync;
    }

    public Func<IUpdateEventObject, ValueTask<bool>> CanHandleEvent { get; init; }
    public Func<IUpdateEventObject, IResponder, Task> HandleAsync { get; init; }
}