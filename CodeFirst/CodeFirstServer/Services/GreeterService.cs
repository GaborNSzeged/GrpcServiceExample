using ProtoBuf.Grpc;
using Shared;
using System.Threading.Tasks;

namespace CodeFirstServer.Services;

public class GreeterService : IGreeterService
{
    public async Task<HelloReply> SayHelloAsync(HelloRequest request, CallContext context = default)
    {
        return new HelloReply { Message = "Hello " + request.Name };
    }

    public async Task<HelloReply> SayHelloClientStreaming(IAsyncEnumerable<HelloRequest> requests, CallContext context = default)
    {
        await foreach (var request in requests)
        {
            Console.WriteLine(request);
        }
        return new HelloReply();
    }

    public async IAsyncEnumerable<HelloReply> SayHelloServerStreaming(HelloRequest request, CallContext context = default)
    {
        await Task.Delay(1000);
        yield return new HelloReply { Message = "Akos" };
        await Task.Delay(1000);
        yield return new HelloReply { Message = "Akos" };
    }
}
