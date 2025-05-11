// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using GrpcPipeService;
using System.IO.Pipes;

var factory = new NamedPipesConnectionFactory();
using GrpcChannel channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
{
    HttpHandler = new SocketsHttpHandler
    {
        ConnectCallback = factory.ConnectAsync
    }
});

Greeter.GreeterClient c = new Greeter.GreeterClient(channel);
using var call = c.SayHelloAsync(new HelloRequest { Name = "Akos" });
HelloReply replay = await call.ResponseAsync;
Console.WriteLine($"Response: {replay.Message}");
Console.ReadLine();

// signatúra a fontos mert delegate-nél lesz megadva
public class NamedPipesConnectionFactory
{
    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context,
                                                CancellationToken cancellationToken = default)
    {
        var clientStream = new NamedPipeClientStream(
            serverName: ".",
            pipeName: "GrpcPipe",
            direction: PipeDirection.InOut,
            options: PipeOptions.WriteThrough | PipeOptions.Asynchronous,
            impersonationLevel: System.Security.Principal.TokenImpersonationLevel.Anonymous
        );

        try
        {
            await clientStream.ConnectAsync(cancellationToken);
            return clientStream;
        }
        catch 
        {
            clientStream.Dispose();
            throw;
        }
    }
}