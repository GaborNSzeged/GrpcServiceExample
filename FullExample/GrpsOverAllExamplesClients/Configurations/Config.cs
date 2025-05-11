using GrpsOverAllExamplesClients.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GrpsOverAllExamplesClients.Configurations
{
    internal static class Config
    {
        static Config()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ClientApplication application = new ClientApplication(serviceCollection);
            ServiceProvider = application.ServiceProvider;
        }

        public static void Init() { }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            serviceCollection.AddSingleton<ISuperShopClientService>(new SuperShopClientService());
        }

        public static IServiceProvider ServiceProvider { get; }
    }
}
