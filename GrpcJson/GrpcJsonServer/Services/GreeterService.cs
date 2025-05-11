using Grpc.Core;
using GrpcJsonServer;

namespace GrpcJsonServer.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public async override Task SayHelloServerStreaming(HelloRequestStreaming request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        for (var i = 0; i < request.Count; i++)
        {
            await responseStream.WriteAsync(new HelloReply { Message = "Hello " + request.Name });
        }
    }
}
