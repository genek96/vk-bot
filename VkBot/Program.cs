using Microsoft.Extensions.Configuration;
using VkLongPolling;
using VkLongPolling.Configuration;
using VkLongPolling.EventHandlers;
using VkLongPolling.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var clientSettings = config.GetRequiredSection("ClientSettings").Get<ClientSettings>();

LongPoller poller = new(clientSettings, new[]
{
    new UpdateEventHandler(
        e => e is NewMessageEvent, async (e, sendResponse) =>
        {
            var message = e as NewMessageEvent;
            await sendResponse(
                message!.Message.FromId,
                "на связи",
                new KeyboardBuilder()
                    .AddTextButton("Хей")
                    .AddTextButton("Хоп")
                    .AddNewButtonsLine()
                    .AddCallbackButton("Magic", new Payload("магия свершилась"), ButtonColor.Positive)
            );
        }),
    new UpdateEventHandler(
        e => e is MessageEvent, async (e, sendResponse) =>
        {
            var message = e as MessageEvent;
            await sendResponse(
                message!.UserId,
                $"Я всё понял: {message.Payload.Text}",
                new KeyboardBuilder()
                    .AddTextButton("Как то так")
            );
        })
});

CancellationTokenSource tokenSource = new();
var pollingTask = poller.StartPollingAsync(tokenSource.Token);

while (Console.ReadLine() != "stop")
{
    Console.WriteLine("To stop app enter 'stop'");
}

tokenSource.Cancel();
await pollingTask;