using Microsoft.Extensions.Configuration;
using VkLongPolling;
using VkLongPolling.Configuration;
using VkLongPolling.EventHandlers;
using VkLongPolling.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var clientSettings = config.GetRequiredSection("ClientSettings").Get<ClientSettings>();

LongPoller poller = new(clientSettings, new []
{
    new UpdateEventHandler(e => e is NewMessageEvent, e =>
    {
        var message = e as NewMessageEvent;
        Console.WriteLine($"Received message: {message.Message.Text}, from: {message.Message.FromId}");
        return Task.CompletedTask;
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