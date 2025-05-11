using ProtoBuf.Grpc;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Shared;

[ServiceContract]
public interface IGreeterService
{
    [OperationContract]
    Task<HelloReply> SayHelloAsync(HelloRequest request, CallContext context = default);

    [OperationContract]
    IAsyncEnumerable<HelloReply> SayHelloServerStreaming(HelloRequest request, CallContext context = default);

    [OperationContract]
    Task<HelloReply> SayHelloClientStreaming(IAsyncEnumerable<HelloRequest> requests, CallContext context = default);
}

[DataContract]
public class HelloRequest
{
    [DataMember(Order = 1)]
    public string Name { get; set; }
}

[DataContract]
public class HelloReply
{
    [DataMember(Order = 1)]
    public string Message { get; set; }
}
