// See https://aka.ms/new-console-template for more information

using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using GrpcRetryClientDemo;
using GrpcRetryDemoServer;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");

ILoggerFactory loggerFactory = LoggerFactory.Create(opts =>
{
    // need nupkg: Microsoft.Extensions.Logging.Console
    opts.AddConsole();
    opts.SetMinimumLevel(LogLevel.Debug);
});

ILogger logger = loggerFactory.CreateLogger<Program>();
logger.LogDebug("This is a debug message.");
logger.LogInformation("This is an information message.");
logger.LogError("Error message");

logger.LogInformation("Press enter to connect to the server.");
Console.ReadLine();

using GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:7055", new GrpcChannelOptions
{
    MaxRetryAttempts = 10, // global settings
    ServiceConfig = new ServiceConfig()
    {
        // retry-ok torlódás vezérlése
        // RetryThrottling = new RetryThrottlingPolicy(){},

        MethodConfigs = { new Grpc.Net.Client.Configuration.MethodConfig()
        {
            // name must be set
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy()
            {
                MaxAttempts = 5,
                InitialBackoff = TimeSpan.FromSeconds(5),
                BackoffMultiplier = 1.5, // if not successful it will multiply the original time
                MaxBackoff = TimeSpan.FromSeconds(14),
                RetryableStatusCodes = { StatusCode.Unavailable }
            }
            //HedgingPolicy = new HedgingPolicy
            //{
            //    MaxAttempts = 5,
            //    HedgingDelay = TimeSpan.FromSeconds(5),
            //    NonFatalStatusCodes = { StatusCode.Unavailable }
            //}
        } }
    }
});
CallInvoker invoker = channel.Intercept(new ClientLoggerInterceptor(loggerFactory));

Greeter.GreeterClient greeterClient = new Greeter.GreeterClient(invoker);


using AsyncUnaryCall<HelloReply> call = greeterClient.SayHelloAsync(new HelloRequest { Name = "Akos" });
try
{
    await call.ResponseAsync;
    var x = await call.ResponseHeadersAsync;
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.ReadLine();




