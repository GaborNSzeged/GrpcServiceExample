using Grpc.Core;
using DIDemo;
using System.Collections.Concurrent;

namespace DIDemo.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private readonly Cache cache;
    public GreeterService(ILogger<GreeterService> logger, Cache cache)
    {
        _logger = logger;
        this.cache = cache;
    }

    public async override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        if (cache.cachedReplies.TryGetValue(request.Name, out var cachedReply))
            return cachedReply;

        BusinessLogic businessLogic = new BusinessLogic();
        var resp = businessLogic.DoBusiness(request.Name);
        cache.cachedReplies.TryAdd(request.Name, resp);
        return resp;
    }
}

public class BusinessLogic
{
    public HelloReply DoBusiness(string name)
    {
        return new HelloReply { Message = "Hello " + name };
    }
}

public class Cache
{
    //public object GetItem(string key) {  }
    public ConcurrentDictionary<string, HelloReply> cachedReplies = new ConcurrentDictionary<string, HelloReply>();
}
