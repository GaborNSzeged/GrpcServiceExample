using GrpcHealthCheckServerDemo.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Inslall: Grpc.AspNetCore.HealthChecks.nupkg
// Több health check is megadható
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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcHealthChecksService();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


app.Run();
