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
        (message, responseSender) => responseSender(
            message.Message.FromId,
            "Хоба",
            new KeyboardBuilder()
                .AddTextButton("Текст")
                .AddCallbackButton("Callback", new Payload("скрытый текст"), ButtonColor.Positive)))
    .AddCallbackHandler(e => true,
        (message, responseSender) => responseSender(
            message.UserId,
            $"Ты отправил мне: {message.Payload.Text}",
            new KeyboardBuilder()
                .AddTextButton("Ещё текст")
                .AddCallbackButton("Callback", new Payload("другой скрытый текст"), ButtonColor.Positive))));

CancellationTokenSource tokenSource = new();
var pollingTask = poller.StartPollingAsync(tokenSource.Token);

while (Console.ReadLine() != "stop")
{
    Console.WriteLine("To stop app enter 'stop'");
}

tokenSource.Cancel();
await pollingTask;