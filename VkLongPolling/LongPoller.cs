using Serilog;
using VkLongPolling.Client;
using VkLongPolling.Configuration;
using VkLongPolling.EventHandlers;
using VkLongPolling.Exceptions;
using VkLongPolling.Models;

namespace VkLongPolling;

public class LongPoller
{
    public LongPoller(
        ClientSettings settings,
        Func<EventHandlersChainBuilder, EventHandlersChainBuilder> builder,
        ILogger logger
    )
    {
        _settings = settings;
        _logger = logger;
        var b = new EventHandlersChainBuilder();
        _handlers = builder(b).Build();
    }

    public async Task StartPollingAsync(CancellationToken cancellationToken)
    {
        using IVkClient client = new VkClient(_settings);
        Responder responder = new(client);

        _logger.Information("Acquiring session info...");
        var sessionInfo = await ExceptionHelper.TryDoWithRethrow(
            () => client.GetLongPollSessionAsync(),
            e => _logger.Fatal(e, "Failed to acquire session info"));
        _logger.Information("Session info was successfully updated. New session server and timestamp: {Server}, {Timestamp}", sessionInfo.Server, sessionInfo.Ts);

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.Debug("Start getting updates");
            var updatesResponse = await ExceptionHelper.TryDoAsync(
                () => client.GetUpdatesAsync(sessionInfo, cancellationToken),
                e => _logger.Error(e, "Failed to get updates"));

            if (updatesResponse == null)
                continue;

            if (updatesResponse.Failed != null)
            {
                _logger.Warning("Updates was not received. Reason: {Reason}", updatesResponse.Failed);
                sessionInfo = await HandleFailureAsync(updatesResponse, client, sessionInfo);
                continue;
            }

            foreach (var updateEvent in updatesResponse.Updates ?? Array.Empty<UpdateEvent>())
            {
                _logger.Debug("Start handling event of type {Type}", updateEvent.Type);
                var handler = await GetEventHandlerAsync(updateEvent);
                if (handler != null)
                {
                    await ExceptionHelper.TryDoAsync(
                        () => handler.HandleAsync(updateEvent.Object, responder),
                        e => _logger.Error(e, "Failed to handle event: {EventId}", updateEvent.Object)
                    );
                    break;
                }

                _logger.Warning("There is no handler for event: {Event}. Event will be skipped", updateEvent.Object);
            }

            sessionInfo = new SessionInfo(sessionInfo.Server, sessionInfo.Key, updatesResponse.Ts!);
        }
    }

    private async Task<UpdateEventHandler?> GetEventHandlerAsync(UpdateEvent @event)
    {
        foreach (var handler in _handlers)
        {
            if (await handler.CanHandleEvent(@event.Object))
            {
                return handler;
            }
        }

        return null;
    }

    private static async Task<SessionInfo> HandleFailureAsync(
        LongPollResponse response,
        IVkClient client,
        SessionInfo currentSession
    ) =>
        response.Failed switch
        {
            1 => new SessionInfo(currentSession.Server, currentSession.Key, response.Ts!),
            2 => await client.GetLongPollSessionAsync(),
            3 => await client.GetLongPollSessionAsync(),
            _ => throw new NetworkException(
                $"Unknown failure occured during getting updates. Failure code: {response.Failed}")
        };

    private readonly ClientSettings _settings;
    private readonly ILogger _logger;
    private readonly UpdateEventHandler[] _handlers;
}