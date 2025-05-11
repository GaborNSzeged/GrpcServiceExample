using Grpc.Core;

namespace DeadLineService.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        await DoBusiness(context.CancellationToken);
        return new HelloReply
        {
            Message = "Hello " + request.Name
        };
    }

    private async Task DoBusiness(CancellationToken cancellationToken)
    {
        await Task.Delay(10000);
        // This will throw an exception if deadLine expired. Otherwise the server does not recognize it.
        cancellationToken.ThrowIfCancellationRequested();
    }

}
