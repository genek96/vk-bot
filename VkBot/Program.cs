using Microsoft.Extensions.Configuration;
using VkLongPolling;
using VkLongPolling.Configuration;
using VkLongPolling.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var clientSettings = config.GetRequiredSection("ClientSettings").Get<ClientSettings>();

LongPoller poller = new(clientSettings, b => b
    .AddNewMessageHandler(e => true,
        (message, responseSender) => responseSender.SendMessageAsync(
            message.Message.FromId,
            "Хоба",
            new KeyboardBuilder()
                .AddTextButton("Текст")
                .AddCallbackButton("Callback", new Payload("скрытый текст"), ButtonColor.Positive)))
    .AddCallbackHandler(e => true,
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