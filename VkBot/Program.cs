using Ninject;
using Serilog;
using VkBot.Configuration;

var container = ContainerConfigurator.Configure();

var poller = container.Get<LongPollerConfigurator>().Configure();

CancellationTokenSource tokenSource = new();
Log.Logger.Information("Application was configured");
var pollingTask = poller.StartPollingAsync(tokenSource.Token);
var commandsReadTask = Task.Run(() =>
{
    while (Console.ReadLine() != "stop")
    {
        Console.WriteLine("To stop app enter 'stop'");
    }

    Log.Logger.Information("Command stop was received");
    tokenSource.Cancel();
});

await Task.WhenAny(pollingTask, commandsReadTask);
Log.Logger.Information("Application stopped");