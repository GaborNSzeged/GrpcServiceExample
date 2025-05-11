using Microsoft.Extensions.Logging;

namespace GrpsOverAllExamplesClients.Services
{
    public class PaymentService : IPaymentService
    {
        public ILogger Logger { get; }
        public PaymentService(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<PaymentService>();
            if (Logger == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            Logger.LogInformation("PaymentService created");
        }
    }
}
