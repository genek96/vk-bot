using Microsoft.Extensions.Configuration;
using Ninject;
using Ninject.Syntax;
using VkBot.Storing;
using VkLongPolling.Configuration;

namespace VkBot.Configuration;

public static class ContainerConfigurator
{
    public static IKernel Configure()
    {
        StandardKernel container = new();
        ConfigureSettings(container);
        ConfigureStorages(container);

        container.Bind<IServiceProvider>().ToConstant(container);
        return container;
    }

    private static void ConfigureSettings(IBindingRoot container)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var clientSettings = config.GetRequiredSection("ClientSettings").Get<ClientSettings>();
        var dbSettings = config.GetRequiredSection("DatabaseSettings").Get<DatabaseSettings>();

        container.Bind<ClientSettings>().ToConstant(clientSettings).InSingletonScope();
        container.Bind<DatabaseSettings>().ToConstant(dbSettings).InSingletonScope();
    }

    private static void ConfigureStorages(IBindingRoot bindingRoot)
    {
        bindingRoot.Bind<IUserStateStorage>().To<UserStateStorage>().InTransientScope();
    }
}