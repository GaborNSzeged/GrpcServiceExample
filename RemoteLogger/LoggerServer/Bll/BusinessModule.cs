using LoggerServer.Helpers;

namespace LoggerServer.Bll
{
    public static class BusinessModule
    {
        public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // It must be singleton to ensure only one thread have access to the IO function.
            services.AddSingleton<IFileHandler, FileHandler>();
        }
    }
}
