// See https://aka.ms/new-console-template for more information

using DeadLineService;
using Grpc.Core;
using Grpc.Net.Client;

Console.WriteLine("Hello, World!");

using GrpcChannel grpcChannel = GrpcChannel.ForAddress("https://localhost:7196");

Greeter.GreeterClient client = new Greeter.GreeterClient(grpcChannel);

var deadLine = DateTime.UtcNow.AddSeconds(5);
var callOptions = new CallOptions(null, deadLine);

AsyncUnaryCall<HelloReply> call = client.SayHelloAsync(new HelloRequest() { Name = "Gabor" }, callOptions);

try
{
    HelloReply response = await call.ResponseAsync;
    Console.WriteLine(response.Message);
}
catch (Exception ex)
{
    // Status(StatusCode="DeadlineExceeded", Detail="")
    Console.WriteLine(ex.Message);
}


Console.ReadLine();
