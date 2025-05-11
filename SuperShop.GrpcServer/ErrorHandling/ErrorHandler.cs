namespace SuperShop.GrpcServer.ErrorHandling
{
    public class ErrorHandler : IErrorHandler
    {
        public async Task HandleError(Exception ex, string correlationId)
        {
            Console.WriteLine($"{correlationId}: {ex}");
        }
    }

}
