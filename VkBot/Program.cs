using Microsoft.Extensions.Configuration;
using VkBot.Storing;
using VkLongPolling;
using VkLongPolling.Configuration;
using VkLongPolling.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var clientSettings = config.GetRequiredSection("ClientSettings").Get<ClientSettings>();

InMemoryUserStateStorage stateRepository = new();
LongPoller poller = new(clientSettings, b => b
    .AddNewMessageHandler(async e =>
    {
        var state = await stateRepository.GetUserStateAsync(e.Message.FromId);
        return state == UserState.Initial;
    }, async (message, responseSender) =>
    {
        await stateRepository.SetUserState(message.Message.FromId, UserState.InProgress);
        await responseSender.SendMessageAsync(
            message.Message.FromId,
            "Приветствую тебя, путник!",
            new KeyboardBuilder()
                .AddTextButton("Текст")
                .AddCallbackButton("Callback", new Payload("скрытый текст"), ButtonColor.Positive));
    })
    .AddNewMessageHandler(async e =>
        {
            var state = await stateRepository.GetUserStateAsync(e.Message.FromId);
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
    )
);

CancellationTokenSource tokenSource = new();
var pollingTask = poller.StartPollingAsync(tokenSource.Token);
var commandsReadTask = Task.Run(() =>
{
    while (Console.ReadLine() != "stop")
    {
        Console.WriteLine("To stop app enter 'stop'");
    }

    tokenSource.Cancel();
});

await Task.WhenAny(pollingTask, commandsReadTask);