using Grpc.Core;

namespace GrpcServiceDepency.Services;

public class GreeterService : Greeter.GreeterBase
{
    //private readonly ILogger<GreeterService> _logger;
    //public GreeterService(ILogger<GreeterService> logger)
    //{
    //    _logger = logger;
    //}

    private readonly IBusinessLogic _businessÍLogic;

    // rpc hívásonkén példányosodnak, as service stub hívásonként példányosodik
    public GreeterService(IBusinessLogic businessÍLogic)
    {
        _businessÍLogic = businessÍLogic ?? throw new ArgumentNullException(nameof(businessÍLogic));
    }

    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        await _businessÍLogic.DoBusiness();

        return new HelloReply
        {
            Message = "Hello " + request.Name
        };
    }
}

public interface IBusinessLogic
{
    Task DoBusiness();
}

public class BusinessLogic : IBusinessLogic
{
    private readonly IService _service;
    private readonly IService2 _service2;
    public BusinessLogic(IService service, IService2 service2, ISuperInterface1 s1, ISuperInterface2 s2)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _service2 = service2 ?? throw new ArgumentNullException(nameof(service2));
    }

    public async Task DoBusiness()
    {

    }
}

public interface IService { }
public class Service : IService
{
    private readonly ICoreService _coreService;

    public Service(ICoreService coreService)
    {
        _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
    }
}


public interface IService2 { }
public class Service2 : IService2
{
    private readonly ICoreService _coreService;

    public Service2(ICoreService coreService)
    {
        _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
    }
}

public interface ICoreService { }

public class CoreService : ICoreService, IDisposable
{
    public CoreService() { }

    public void Dispose() { }
}

public class CoreService2 : ICoreService
{
    public CoreService2() { }
}

public interface ISuperInterface1 { }

public interface ISuperInterface2 { }

public class SuperImplementation : ISuperInterface1, ISuperInterface2
{
    public SuperImplementation() { }
}
