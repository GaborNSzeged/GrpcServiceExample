using GrpsOverAllExamplesClients.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SuperShopServer.General;
using SuperShopServer.Services;

#region URL config
Host.CreateDefaultBuilder(args)
           .ConfigureWebHostDefaults(webBuilder =>
           {
               webBuilder.UseStartup<Program>();

               var configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build();

               var urls = configuration.GetSection("Kestrel:Endpoints").GetChildren()
                   .Select(endpoint => endpoint.GetValue<string>("Url"))
                   .ToArray();

               webBuilder.UseUrls(urls);
           });
#endregion

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

#region Health Check
builder.Services.AddGrpcHealthChecks(opts =>
{
    opts.Services.Map(ServiceHealthTypes.Cpu.ToString(), check =>
    {
        return check.Tags.Contains(ServiceHealthTypes.Cpu.ToString());
    });
    opts.Services.Map(ServiceHealthTypes.Disk.ToString(), check =>
    {
        return check.Tags.Contains(ServiceHealthTypes.Disk.ToString());
    });
    opts.Services.Map(ServiceHealthTypes.Memory.ToString(), check =>
    {
        return check.Tags.Contains(ServiceHealthTypes.Memory.ToString());
    });
})
    .AddCheck("Demo1", () =>
    {
        if (SystemInfo.Cpu < 49)
        {
            return HealthCheckResult.Healthy();
        }

        if (SystemInfo.Cpu < 89)
        {
            return HealthCheckResult.Degraded();
        }

        return HealthCheckResult.Unhealthy();
    }, [ServiceHealthTypes.Cpu.ToString()])
    .AddCheck("Demo2", () =>
    {
        if (SystemInfo.Disk < 49)
        {
            return HealthCheckResult.Healthy();
        }

        if (SystemInfo.Disk < 89)
        {
            return HealthCheckResult.Degraded();
        }

        return HealthCheckResult.Unhealthy();
    }, [ServiceHealthTypes.Disk.ToString()])
    .AddCheck("Demo3", () =>
    {
        if (SystemInfo.Memory < 49)
        {
            return HealthCheckResult.Healthy();
        }

        if (SystemInfo.Memory < 89)
        {
            return HealthCheckResult.Degraded();
        }

        return HealthCheckResult.Unhealthy();
    }, [ServiceHealthTypes.Memory.ToString()]);


builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.Zero; // default is 5 min
    options.Period = TimeSpan.FromSeconds(5);
});

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcHealthChecksService().AllowAnonymous();
app.MapGrpcService<GreeterService>();
app.MapGrpcService<MockValueSetterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
