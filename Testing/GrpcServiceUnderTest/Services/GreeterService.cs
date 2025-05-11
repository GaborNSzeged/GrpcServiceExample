using Grpc.Core;
using GrpcServiceUnderTest;

namespace GrpcServiceUnderTest.Services;

public interface IBusinessService { }
public class BusinessService : IBusinessService { }

public class GreeterService : Greeter.GreeterBase
{
    private readonly IBusinessService businessService;
    public GreeterService(IBusinessService businessService)
    {
        this.businessService = businessService;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override async Task<HelloReply> SayHelloClientStreaming(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        await foreach (var item in requestStream.ReadAllAsync())
        {
            
        }
        return new HelloReply { Message = "Gabor" };
    }
}
