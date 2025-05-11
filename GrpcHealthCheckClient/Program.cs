// See https://aka.ms/new-console-template for more information

using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;

Console.WriteLine("Hello, World!");

GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:7246");
Health.HealthClient client = new Health.HealthClient(channel);

// This solution is good if you wan to get the health check only once.
var checkCall = client.CheckAsync(new HealthCheckRequest());
var resp = await checkCall.ResponseAsync;
Console.WriteLine(resp.Status);


// Continuous checking
using var cts = new CancellationTokenSource();
using var watchCall = client.Watch(new HealthCheckRequest() { Service="x2"}, cancellationToken: cts.Token);
var watchTask = Task.Run(async () =>
{
    try
    {
        await foreach (var message in watchCall.ResponseStream.ReadAllAsync())
        {
            // ide csak akkor jövünk, ha változás történt a server oldalon
            Console.WriteLine("Health of the server: " + message.Status);
        }
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
    {
        // cancelled exception is swollowed as it was thrown deliberatelly
    }
});

Console.WriteLine("Pres enter to exit");

Console.ReadLine();

// stop the watch service
cts.Cancel();
await watchTask;
