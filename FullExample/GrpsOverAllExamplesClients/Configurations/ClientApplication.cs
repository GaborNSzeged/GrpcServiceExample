using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GrpsOverAllExamplesClients.Configurations
{
    public class ClientApplication
    {
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger Logger { get; set; }

        public ClientApplication(IServiceCollection serviceCollection)
        {
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            Logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ClientApplication>();
            Logger.LogInformation("Application created successfully.");
        }

        //public void MakePayment(PaymentDetails paymentDetails)
        //{
        //    Logger.LogInformation($"Begin making a payment {paymentDetails}");
        //    IPaymentService paymentService = ServiceProvider.GetRequiredService<IPaymentService>();
        //    // ...
        //}

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            
            //serviceCollection.AddSingleton<IPaymentService, PaymentService>();
        }
    }
}
