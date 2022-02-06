using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VkBot.Storing;
using VkBot.Storing.Models;
using VkLongPolling;
using VkLongPolling.Configuration;
using VkLongPolling.EventHandlers;
using VkLongPolling.Models;

namespace VkBot.Configuration;

public class LongPollerConfigurator
{
    public LongPollerConfigurator(IServiceProvider container)
    {
        _container = container;
    }

    public LongPoller Configure()
    {
        return new LongPoller(_container.GetService<ClientSettings>()!, ConfigureBuilder, Log.Logger);
    }

    private EventHandlersChainBuilder ConfigureBuilder(EventHandlersChainBuilder b)
    {
        return b.AddNewMessageHandler(async e =>
            {
                using var stateRepository = _container.GetService<UserStateStorage>();
                var state = await stateRepository!.GetUserStateAsync(e.Message.FromId);
                return state == UserState.Initial;
            }, async (message, responseSender) =>
            {
                using var stateRepository = _container.GetService<UserStateStorage>();
                await stateRepository!.SetUserState(message.Message.FromId, UserState.InProgress);
                await responseSender.SendMessageAsync(
                    message.Message.FromId,
                    "Приветствую тебя, путник!",
                    new KeyboardBuilder()
                        .AddTextButton("Текст")
                        .AddCallbackButton("Callback", new Payload("скрытый текст"), ButtonColor.Positive));
            })
            .AddNewMessageHandler(async e =>
                {
                    using var stateRepository = _container.GetService<UserStateStorage>();
                    var state = await stateRepository!.GetUserStateAsync(e.Message.FromId);
                    return state == UserState.InProgress;
                },
                (message, responseSender) => responseSender.SendMessageAsync(
                    message.Message.FromId,
                    "Ну всё, давай уёбывай уже.",
                    new KeyboardBuilder()
                        .AddTextButton("Текст")
                        .AddCallbackButton("Callback", new Payload("скрытый текст"), ButtonColor.Positive)))
            .AddCallbackHandler(e => ValueTask.FromResult(true),
                (message, responseSender) => responseSender.SendMessageEventAnswerAsync(
                    message.UserId,
                    message.EventId,
                    new SnackbarAnswer("Хобана"))
            );
    }

    private readonly IServiceProvider _container;
}