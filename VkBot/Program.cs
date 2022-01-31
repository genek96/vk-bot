using Ninject;
using VkBot.Configuration;

var container = ContainerConfigurator.Configure();

var poller = container.Get<LongPollerConfigurator>().Configure();

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