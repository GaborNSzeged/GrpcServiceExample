using Grpc.Core;
using GrpcRetryDemoServer;

namespace GrpcRetryDemoServer.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private static Random random = new Random();

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        if (random.Next() % 2 == 0)
        {
            throw new InvalidOperationException();
        }

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
