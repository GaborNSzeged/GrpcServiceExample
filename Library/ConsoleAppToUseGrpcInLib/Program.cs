using System;
using System.Threading;

Console.WriteLine("Start Greeting server!");
var server = new GrpcInLib.Server();
server.Start();

Console.WriteLine("Enter 'quit' to stop the server/n");

do
{
    string? exit = Console.ReadLine();
    if (exit == "quit")
    {
        server.Stop();
        break;
    }
    Thread.Sleep(1000);
} while (true);


//static void Main(string[] args)
//{
//    var config = new ConfigurationBuilder()
//        .AddJsonFile("appsettings.json")
//        .Build();

//    var grpcOptions = new GrpcServerOptions();
//    config.GetSection("Grpc").Bind(grpcOptions);

//    var services = new ServiceCollection();
//    services.AddSingleton(grpcOptions);

//    var serviceProvider = services.BuildServiceProvider();

//    GrpcHost.Start(serviceProvider);
//}

// GrpcHost.Start
//public static void Start(IServiceProvider serviceProvider)
//{
//    var options = serviceProvider.GetRequiredService<GrpcServerOptions>();
//}

Console.WriteLine("Greeting server stopped");

