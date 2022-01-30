using VkLongPolling.Client;
using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class SpecificEventHandler<T> : UpdateEventHandler where T: class, IUpdateEventObject
{
    public SpecificEventHandler(
        Func<T, ValueTask<bool>> canHandleEvent,
        Func<T, IResponder, Task> handleAsync)
        : base(
            async e => e is T eventObject && await canHandleEvent(eventObject),
            (e, handle) => handleAsync(e as T, handle))
    {
    }
}