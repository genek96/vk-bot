using VkLongPolling.Client;
using VkLongPolling.Models;

namespace VkLongPolling.EventHandlers;

public class EventHandlersChainBuilder
{
    public EventHandlersChainBuilder()
    {
        _handlers = new List<UpdateEventHandler>();
    }

    public EventHandlersChainBuilder AddNewMessageHandler(
        Predicate<NewMessageEvent> canHandleEvent,
        Func<NewMessageEvent, IResponder, Task> handleAsync
    )
    {
        _handlers.Add(new SpecificEventHandler<NewMessageEvent>(canHandleEvent, handleAsync));
        return this;
    }

    public EventHandlersChainBuilder AddCallbackHandler(
        Predicate<MessageEvent> canHandleEvent,
        Func<MessageEvent, IResponder, Task> handleAsync
    )
    {
        _handlers.Add(new SpecificEventHandler<MessageEvent>(canHandleEvent, handleAsync));
        return this;
    }

    public UpdateEventHandler[] Build()
    {
        return _handlers.ToArray();
    }

    private List<UpdateEventHandler> _handlers;
}