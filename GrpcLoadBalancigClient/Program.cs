// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using GrpcLoadBalancigHost1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");

// ha a dns van átírva
//DnsResolverFactory dnsResolverFactory = new DnsResolverFactory();

// lehet egyet vagy akár többet is használni
StaticResolverFactory factory = new StaticResolverFactory(adress =>
{
    return new[] { new BalancerAddress("localhost", 7007), new BalancerAddress("localhost", 7273) };
});

//RoundRobinBalancerFactory
//PickFirstBalancerFactory

// mini DI container
//ServiceCollection services = new ServiceCollection();
//services.AddSingleton<ResolverFactory>(factory);

//using GrpcChannel channel = GrpcChannel.ForAddress("static:///anyAddress",
//    new GrpcChannelOptions
//    {
//        Credentials = ChannelCredentials.SecureSsl, // Htts 
//        ServiceProvider = services.BuildServiceProvider(), // for static resolver factory
//        ServiceConfig = new ServiceConfig
//        {
//            LoadBalancingConfigs = { new PickFirstConfig() } //  new RoundRobinConfig()
//        }
//    });

ServiceCollection services = new ServiceCollection();
services.AddSingleton<ResolverFactory>(new FileResolverFactory());
services.AddSingleton<LoadBalancerFactory>(new RandomLoadBalancerFactory());

using GrpcChannel channel = GrpcChannel.ForAddress(
    "file:///D:\\futokepzes\\SL05\\Solution1\\GrpcLoadBalancingClient\\addresses.txt",
    new GrpcChannelOptions
    {
        Credentials = ChannelCredentials.SecureSsl,
        ServiceProvider = services.BuildServiceProvider(),
        ServiceConfig = new Grpc.Net.Client.Configuration.ServiceConfig
        {
            LoadBalancingConfigs = { new RandomBalancingConfig() /*new LoadBalancingConfig("random")*/ }
        }
    });

var client = new Greeter.GreeterClient(channel);
var c1 = client.SayHelloAsync(new HelloRequest() { Name = "X" });
await c1.ResponseAsync;

Console.ReadLine();

var c2 = client.SayHelloAsync(new HelloRequest() { Name = "X" });
await c1.ResponseAsync;


public class FileResolverFactory : ResolverFactory
{
    public override string Name => "file";
    public override Resolver Create(ResolverOptions options)
    {
        return new FileResolver(options.Address, options.DefaultPort, options.LoggerFactory);
    }
}

public class FileResolver : PollingResolver
{
    private Uri address;
    private int defaultPort;
    private ILoggerFactory loggerFactory;

    public FileResolver(Uri address, int defaultPort, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        this.address = address;
        this.defaultPort = defaultPort;
        this.loggerFactory = loggerFactory;
    }

    protected override async Task ResolveAsync(CancellationToken cancellationToken)
    {
        List<BalancerAddress> balancerAddresses = new List<BalancerAddress>();
        await foreach (string line in File.ReadLinesAsync(address.LocalPath))
        {
            string[] parts = line.Split(":");
            if (parts.Length == 2)
            {
                BalancerAddress item = new(parts[0], int.Parse(parts[1]));
                balancerAddresses.Add(item);
                Console.WriteLine($"Resolved: {item}");
            }
            else
            {
                BalancerAddress item = new(parts[0], defaultPort);
                balancerAddresses.Add(item);
                Console.WriteLine($"Resolved: {item}");
            }
        }
        Listener.Invoke(ResolverResult.ForResult(balancerAddresses));
    }
}

public class RandomBalancingConfig : LoadBalancingConfig
{
    public static readonly string PolicyName = "random";
    public RandomBalancingConfig() : base(PolicyName)
    {
    }
}

public class RandomLoadBalancerFactory : LoadBalancerFactory
{
    public override string Name => RandomBalancingConfig.PolicyName;

    public override LoadBalancer Create(LoadBalancerOptions options)
    {
        return new RandomBalancer(options.Controller, options.LoggerFactory);
    }
}

internal class RandomBalancer : SubchannelsLoadBalancer
{
    private IChannelControlHelper controller;
    private ILoggerFactory loggerFactory;

    public RandomBalancer(IChannelControlHelper controller, ILoggerFactory loggerFactory) : base(controller, loggerFactory)
    {
        this.controller = controller;
        this.loggerFactory = loggerFactory;
    }

    protected override SubchannelPicker CreatePicker(IReadOnlyList<Subchannel> readySubchannels)
    {
        return new RandomSubchannelPicker(readySubchannels);
    }
}

public class RandomSubchannelPicker : SubchannelPicker
{
    private IReadOnlyList<Subchannel> readySubchannels;
    private readonly Random random = new Random();

    public RandomSubchannelPicker(IReadOnlyList<Subchannel> readySubchannels)
    {
        this.readySubchannels = readySubchannels;
    }

    public override PickResult Pick(PickContext context)
    {
        Subchannel subchannel = readySubchannels[random.Next() % readySubchannels.Count];
        Console.WriteLine(subchannel.CurrentAddress);
        return PickResult.ForSubchannel(subchannel);
    }
}