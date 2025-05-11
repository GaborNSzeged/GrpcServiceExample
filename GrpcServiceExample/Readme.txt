TODO
 - create useful HealthCheck. Why don't I got Degraded status?
======================================================================================================================================
- Client (gRPC)
    install: Grpc.Net.Client -> for Creating the channel
    install: Grpc.Tools -> for building protobuf
======================================================================================================================================
- Url configuration for server
     Added to the appsettins.json so the sever can be started without VS.
     "Endpoints": {
          "Http": {
            "Url": "http://localhost:5005"
          },
          "Https": {
            "Url": "https://localhost:5006"
          }
        }
======================================================================================================================================
- Health Check
    (only status Healty or Unhealty is reported to the client, Degraded -> Healthy)
    Server
    Install: Grpc.AspNetCore.HealthChecks.nupkg

     If an app has been configured to require authorization by default, configure the gRPC health checks endpoint with AllowAnonymous
     to skip authentication and authorization.
    app.MapGrpcHealthChecksService().AllowAnonymous();

    - config the service
    builder.Services.AddGrpcHealthChecks(opts =>
{
    opts.Services.Map("x", check =>
    {
        // Client can call the server with HealthCheckRequest() { Service="x"}, which means only those healt check will be sent to the
        // client with which it returned true. If the clienc does not specify the name of the server then all the check will be available
        // for the server.
        //
        // HealthCheckMapContext
        // "Demo" won't be included in the health checks for service "x"
        // "Demo2" has tags ["a", "b"], so check.Tags.Contains("a") will return true for it, making "Demo2" part of the health checks for service "x".
        return check.Tags.Contains("a");
    });
    opts.Services.Map("x2", check => true);
})
    .AddCheck("Demo", () => Random.Shared.Next() % 5 == 0 ? HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy())
    .AddCheck("Demo2", () => HealthCheckResult.Healthy(), ["a", "b"]);


builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.Zero; // default is 5 min
    options.Period = TimeSpan.FromSeconds(5);
});

    - Register the service
    app.MapGrpcHealthChecksService();

    Client
    Inslall: Grpc.HealthCheck.nupkg
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
======================================================================================================================================
- DI
    Client:
    Install: Microsoft.Extensions.DependencyInjection.Abstractions.nupkg
    Install: Microsoft.Extensions.Hosting

    Server:
    It comes with gRPC packagtes
======================================================================================================================================
