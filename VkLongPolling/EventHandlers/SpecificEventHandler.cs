using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class SpecificEventHandler<T> : UpdateEventHandler where T: class, IUpdateEventObject
{
    public SpecificEventHandler(
        Predicate<T> canHandleEvent,
        Func<T, SendResponseFunc, Task> handleAsync)
        : base(
            e => e is T newMessageEvent && canHandleEvent(newMessageEvent),
            (e, handle) => handleAsync(e as T, handle))
    {
    }
}