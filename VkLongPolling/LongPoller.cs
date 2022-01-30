﻿using VkLongPolling.Client;
using VkLongPolling.Configuration;
using VkLongPolling.EventHandlers;
using VkLongPolling.Exceptions;
using VkLongPolling.Models;

namespace VkLongPolling;

public class LongPoller
{
    public LongPoller(ClientSettings settings, Func<EventHandlersChainBuilder, EventHandlersChainBuilder> builder)
    {
        _settings = settings;
        var b = new EventHandlersChainBuilder();
        _handlers = builder(b).Build();
    }

    public async Task StartPollingAsync(CancellationToken cancellationToken)
    {
        using VkClient client = new(_settings);
        Responder responder = new(client);

        var sessionInfo = await client.GetLongPollSessionAsync();
        while (!cancellationToken.IsCancellationRequested)
        {
            var updatesResponse = await client.GetUpdatesAsync(sessionInfo, cancellationToken);
            if (updatesResponse == null)
                continue;

            if (updatesResponse.Failed != null)
            {
                sessionInfo = await HandleFailureAsync(updatesResponse, client, sessionInfo);
                continue;
            }

            foreach (var updateEvent in updatesResponse.Updates ?? Array.Empty<UpdateEvent>())
            {
                var handler = await GetEventHandlerAsync(updateEvent);
                if (handler != null)
                {
                    await handler.HandleAsync(updateEvent.Object, responder);
                    break;
                }
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
    private readonly UpdateEventHandler[] _handlers;
}