// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Shared;


using GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:7238");
var client = channel.CreateGrpcService<IGreeterService>();

var response = await client.SayHelloAsync(new HelloRequest { Name = "Akos" });

var r2 = await client.SayHelloClientStreaming(GetRequests());

await foreach (var res in client.SayHelloServerStreaming(new HelloRequest { Name="Akos"}))
{
    Console.WriteLine(res);
}

Console.WriteLine(response);
Console.ReadLine();

static async IAsyncEnumerable<HelloRequest> GetRequests()
{
    await Task.Delay(1000);
    yield return new HelloRequest { Name = "Akos" };
    await Task.Delay(1000);
    yield return new HelloRequest { Name = "Akos" };
}